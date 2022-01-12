using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        string iv = "0123456789012345";
        string keyName = "keyName";

        //private List<Bank_Account> accounts;
        public static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            //accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        private SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase()
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance TEXT NOT NULL,
                        accountType TEXT NOT NULL,
                        overdraftAmount TEXT,
                        interestRate TEXT
                    ) WITHOUT ROWID
                ";

                command.ExecuteNonQuery();
                
            }
        }
        //
        

        public Bank_Account GetBankAccount(string acc)
        {

            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {
                using (var connection = getDatabaseConnection()) //Initialize database
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.Add(new SqliteParameter("@accountNo",KeyEncryptions.encryptWithKey(acc,keyName,iv)));

                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {

                        int accountType = int.Parse(KeyEncryptions.decryptWithKey(dr.GetString(7), keyName, iv));

                        if (accountType == Account_Type.Current_Account)
                        {//Here is all the decryption for information
                            Current_Account ca = new Current_Account();
                            ca.accountNo = KeyEncryptions.decryptWithKey(dr.GetString(0), keyName, iv);
                            ca.name = KeyEncryptions.decryptWithKey(dr.GetString(1), keyName, iv);
                            ca.address_line_1 = KeyEncryptions.decryptWithKey(dr.GetString(2),keyName,iv);
                            ca.address_line_2 = KeyEncryptions.decryptWithKey(dr.GetString(3), keyName, iv);
                            ca.address_line_3 = KeyEncryptions.decryptWithKey(dr.GetString(4), keyName, iv);
                            ca.town = KeyEncryptions.decryptWithKey(dr.GetString(5), keyName, iv);
                            ca.balance = double.Parse(KeyEncryptions.decryptWithKey(dr.GetString(6), keyName, iv));
                            ca.overdraftAmount = double.Parse(KeyEncryptions.decryptWithKey(dr.GetString(8), keyName, iv));
                            return ca;
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = KeyEncryptions.decryptWithKey(dr.GetString(0), keyName, iv);
                            sa.name = KeyEncryptions.decryptWithKey(dr.GetString(1), keyName, iv);
                            sa.address_line_1 = KeyEncryptions.decryptWithKey(dr.GetString(2), keyName, iv);
                            sa.address_line_2 = KeyEncryptions.decryptWithKey(dr.GetString(3), keyName, iv);
                            sa.address_line_3 = KeyEncryptions.decryptWithKey(dr.GetString(4), keyName, iv);
                            sa.town = KeyEncryptions.decryptWithKey(dr.GetString(5), keyName, iv);
                            sa.balance = double.Parse(KeyEncryptions.decryptWithKey(dr.GetString(6), keyName, iv));
                            sa.interestRate = double.Parse(KeyEncryptions.decryptWithKey(dr.GetString(9), keyName, iv));
                            return sa;
                        }
                    }
                }


            }

            return null;
        }



        public String addBankAccount(Bank_Account ba) 
        {
            int acctype;

            if (ba.GetType() == typeof(Current_Account))
            {
                acctype = 1;
            }
            else
            {
                acctype = 2;
            }


            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO Bank_Accounts VALUES (" +
                    "@accountNo," +
                    "@name," +
                    "@address1," +
                    "@address2," +
                    "@address3," +
                    "@town," +
                    "@balance," +
                    "@accountType,"; // this prevents SQL injections
                //Here I am calling the KeyEncryptions class to encrypt all of the data
                command.Parameters.Add(new SqliteParameter("@accountNo",KeyEncryptions.encryptWithKey(ba.accountNo,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@name",KeyEncryptions.encryptWithKey(ba.name,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@address1", KeyEncryptions.encryptWithKey(ba.address_line_1,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@address2", KeyEncryptions.encryptWithKey(ba.address_line_2,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@address3", KeyEncryptions.encryptWithKey(ba.address_line_3,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@town", KeyEncryptions.encryptWithKey(ba.town,keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@balance",KeyEncryptions.encryptWithKey(ba.balance.ToString(),keyName,iv)));
                command.Parameters.Add(new SqliteParameter("@accountType", KeyEncryptions.encryptWithKey(acctype.ToString(),keyName,iv)));


                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.CommandText += "@overdraftAmount,NULL)";
                    command.Parameters.Add(new SqliteParameter("@overdraftAmount",KeyEncryptions.encryptWithKey(ca.overdraftAmount.ToString(),keyName,iv)));
                }

                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.CommandText += "NULL,@interestRate)";
                    command.Parameters.Add(new SqliteParameter("@interestRate", KeyEncryptions.encryptWithKey(sa.interestRate.ToString(),keyName,iv)));
                }

                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }

        public bool closeBankAccount(String accNo) 
        {

            Bank_Account toRemove = null;
            toRemove = GetBankAccount(accNo);
            
            if (toRemove == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = @accountNo";
                    command.Parameters.Add(new SqliteParameter("@accountNo", KeyEncryptions.encryptWithKey(accNo,keyName,iv)));
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            Bank_Account toLodgeTo = null;
            toLodgeTo = GetBankAccount(accNo);
            double newAmount = toLodgeTo.balance + amountToLodge;
           
            if (toLodgeTo == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.Add(new SqliteParameter("@balance", KeyEncryptions.encryptWithKey(newAmount.ToString(), keyName,iv)));
                    command.Parameters.Add(new SqliteParameter("@accountNo", KeyEncryptions.encryptWithKey(accNo,keyName,iv)));
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            Bank_Account toWithdrawFrom = null;

            toWithdrawFrom = GetBankAccount(accNo);
            double newBalance = toWithdrawFrom.balance - amountToWithdraw;

            if (toWithdrawFrom == null)
                return false;
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = @balance WHERE accountNo = @accountNo";
                    command.Parameters.Add(new SqliteParameter("@balance", KeyEncryptions.encryptWithKey(newBalance.ToString(),keyName,iv)));
                    command.Parameters.Add(new SqliteParameter("@accountNo", KeyEncryptions.encryptWithKey(accNo,keyName,iv)));
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }
        //caa7eba8-a305-4e04-a61d-1bb55a04c7e0
    }
}
