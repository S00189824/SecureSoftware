using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Security.Cryptography;

namespace Banking_Application
{
    public class Program
    {
        

        public static void Main(string[] args)
        {

            Data_Access_Layer dal = Data_Access_Layer.getInstance();
            bool running = true;
            running = BankMenu(dal, running);

            //First have the data that needs to be encrypted here

            //perform Encryption here

            //Perform Decryption here

            
        }
        public static byte[] Bank_Acc_Obj_ToByte(Bank_Account Acc)
        {
            BinaryFormatter binary_formatter = new BinaryFormatter();
            MemoryStream memory_stream = new MemoryStream();
            binary_formatter.Serialize(memory_stream, Acc);
            return memory_stream.ToArray();
        }

        public static Bank_Account Byte_Array_To_Account_Obj(byte[] arrBytes)
        {
            MemoryStream memory_stream = new MemoryStream();
            BinaryFormatter binary_formatter = new BinaryFormatter();
            memory_stream.Write(arrBytes, 0, arrBytes.Length);
            memory_stream.Seek(0, SeekOrigin.Begin);
            Object o = binary_formatter.Deserialize(memory_stream);

            if (o is Bank_Account)//Checks If The Object o Is In Fact An Instance Of The Student Class - If So, Return A Student Object
                return (Bank_Account)o;
            else//If Not - Then Return Null - Note That You Also Have The Option Of Throwing An Exception At This Point.
                return null;
        }

        private static bool BankMenu(Data_Access_Layer dal, bool running)
        {
            do
            {

                Console.WriteLine("");
                Console.WriteLine("***Banking Application Menu***");
                Console.WriteLine("1. Add Bank Account");
                Console.WriteLine("2. Close Bank Account");
                Console.WriteLine("3. View Account Information");
                Console.WriteLine("4. Make Lodgement");
                Console.WriteLine("5. Make Withdrawal");
                Console.WriteLine("6. Exit");
                Console.WriteLine("CHOOSE OPTION:");
                String option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        String accountType = "";
                        int loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");

                            Console.WriteLine("");
                            Console.WriteLine("***Account Types***:");
                            Console.WriteLine("1. Current Account.");
                            Console.WriteLine("2. Savings Account.");
                            Console.WriteLine("CHOOSE OPTION:");
                            accountType = Console.ReadLine();

                            loopCount++;

                        } while (!(accountType.Equals("1") || accountType.Equals("2")));

                        String name = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID NAME ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Name: ");
                            name = Console.ReadLine();

                            loopCount++;

                        } while (name.Equals(""));

                        String addressLine1 = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID ÀDDRESS LINE 1 ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Address Line 1: ");
                            addressLine1 = Console.ReadLine();

                            loopCount++;

                        } while (addressLine1.Equals(""));

                        Console.WriteLine("Enter Address Line 2: ");
                        String addressLine2 = Console.ReadLine();

                        Console.WriteLine("Enter Address Line 3: ");
                        String addressLine3 = Console.ReadLine();

                        String town = "";
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID TOWN ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Town: ");
                            town = Console.ReadLine();

                            loopCount++;

                        } while (town.Equals(""));

                        double balance = -1;
                        loopCount = 0;

                        do
                        {

                            if (loopCount > 0)
                                Console.WriteLine("INVALID OPENING BALANCE ENTERED - PLEASE TRY AGAIN");

                            Console.WriteLine("Enter Opening Balance: ");
                            String balanceString = Console.ReadLine();

                            try
                            {
                                balance = Convert.ToDouble(balanceString);
                            }

                            catch
                            {
                                loopCount++;
                            }

                        } while (balance < 0);

                        Bank_Account ba;

                        if (Convert.ToInt32(accountType) == Account_Type.Current_Account)
                        {
                            double overdraftAmount = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID OVERDRAFT AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Overdraft Amount: ");
                                String overdraftAmountString = Console.ReadLine();

                                try
                                {
                                    overdraftAmount = Convert.ToDouble(overdraftAmountString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (overdraftAmount < 0);

                            ba = new Current_Account(name, addressLine1, addressLine2, addressLine3, town, balance, overdraftAmount);
                        }

                        else
                        {

                            double interestRate = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID INTEREST RATE ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Interest Rate: ");
                                String interestRateString = Console.ReadLine();

                                try
                                {
                                    interestRate = Convert.ToDouble(interestRateString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (interestRate < 0);

                            ba = new Savings_Account(name, addressLine1, addressLine2, addressLine3, town, balance, interestRate);
                        }

                        String accNo = dal.addBankAccount(ba);

                        Console.WriteLine("New Account Number Is: " + accNo);

                        break;
                    case "2":
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.GetBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());

                            String ans = "";

                            do
                            {

                                Console.WriteLine("Proceed With Delection (Y/N)?");
                                ans = Console.ReadLine();

                                switch (ans)
                                {
                                    case "Y":
                                    case "y":
                                        dal.closeBankAccount(accNo);
                                        break;
                                    case "N":
                                    case "n":
                                        break;
                                    default:
                                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                                        break;
                                }
                            } while (!(ans.Equals("Y") || ans.Equals("y") || ans.Equals("N") || ans.Equals("n")));
                        }

                        break;
                    case "3":
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.GetBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            Console.WriteLine(ba.ToString());
                        }

                        break;
                    case "4": //Lodge
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.GetBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToLodge = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Lodge: ");
                                String amountToLodgeString = Console.ReadLine();

                                try
                                {
                                    amountToLodge = Convert.ToDouble(amountToLodgeString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (amountToLodge < 0);

                            dal.lodge(accNo, amountToLodge);
                        }
                        break;
                    case "5": //Withdraw
                        Console.WriteLine("Enter Account Number: ");
                        accNo = Console.ReadLine();

                        ba = dal.GetBankAccount(accNo);

                        if (ba is null)
                        {
                            Console.WriteLine("Account Does Not Exist");
                        }
                        else
                        {
                            double amountToWithdraw = -1;
                            loopCount = 0;

                            do
                            {

                                if (loopCount > 0)
                                    Console.WriteLine("INVALID AMOUNT ENTERED - PLEASE TRY AGAIN");

                                Console.WriteLine("Enter Amount To Withdraw (€" + ba.getAvailableFunds() + " Available): ");
                                String amountToWithdrawString = Console.ReadLine();

                                try
                                {
                                    amountToWithdraw = Convert.ToDouble(amountToWithdrawString);
                                }

                                catch
                                {
                                    loopCount++;
                                }

                            } while (amountToWithdraw < 0);

                            bool withdrawalOK = dal.withdraw(accNo, amountToWithdraw);

                            if (withdrawalOK == false)
                            {

                                Console.WriteLine("Insufficient Funds Available.");
                            }
                        }
                        break;
                    case "6":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("INVALID OPTION CHOSEN - PLEASE TRY AGAIN");
                        break;
                }


            } while (running != false);
            return running;
        }



    }
}