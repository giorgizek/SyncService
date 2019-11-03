﻿using System;
using System.Configuration;
using Zek.Data;
using Zek.Extensions;
using Zek.Security;
using System.Linq;

namespace Sync.Win
{
    public class AppConfig
    {
        private const string Salt = "#Sync@ppS@lt20140123!";
        private const string Key = "GPI_Sync@pp_2014";
        private const string IV = "20131001";


        internal static string GPIH_Server { get; set; }
        internal static string GPIH_Database { get; set; }
        internal static string GPIH_User { get; set; }
        internal static string GPIH_Password { get; set; }

        internal static string CRM_Server { get; set; }
        internal static string CRM_Database { get; set; }
        internal static string CRM_User { get; set; }
        internal static string CRM_Password { get; set; }

        internal static int TakeCount { get; set; }


        internal static int CreatorID { get; set; }
        internal static int CoordinatorID { get; set; }
        internal static int BranchID { get; set; }
        internal static int DeclaratorID { get; set; }
        internal static int PaymentReceiverID { get; set; }
        internal static int PaymentReceiverBankAccountID { get; set; }
        internal static int ProviderID { get; set; }
        internal static int[] AppointmentServiceProviders { get; set; }
        internal static decimal OurDiscountReal { get; set; }


        internal static string ServiceName
        {
            get { return ConfigurationManager.AppSettings["ServiceName"]; }
        }
        internal static string ServiceDisplayName
        {
            get { return ConfigurationManager.AppSettings["ServiceDisplayName"]; }
        }
        internal static string ServiceDescription
        {
            get { return ConfigurationManager.AppSettings["ServiceDescription"]; }
        }

        public static void Load()
        {
            CreatorID = ConfigurationManager.AppSettings["CreatorID"].ToInt32();
            CoordinatorID = ConfigurationManager.AppSettings["CoordinatorID"].ToInt32();
            BranchID = ConfigurationManager.AppSettings["BranchID"].ToInt32();
            DeclaratorID = ConfigurationManager.AppSettings["DeclaratorID"].ToInt32();
            PaymentReceiverID = ConfigurationManager.AppSettings["PaymentReceiverID"].ToInt32();
            PaymentReceiverBankAccountID = ConfigurationManager.AppSettings["PaymentReceiverBankAccountID"].ToInt32();
            ProviderID = ConfigurationManager.AppSettings["ProviderID"].ToInt32();

            var providers = string.Empty;
            foreach (var c in ConfigurationManager.AppSettings["AppointmentServiceProviders"])
            {
                if (char.IsNumber(c) || c == ';' || c == ',' || c == '|' || c == '/' || c == '-' || c == '_')
                    providers += c;
            }
            AppointmentServiceProviders = Array.ConvertAll(providers.Split(new[] { ';', ',', '|', '/', '-', '_' }, StringSplitOptions.RemoveEmptyEntries), int.Parse);

            OurDiscountReal = ConfigurationManager.AppSettings["OurDiscountReal"].ToDecimal();


            GPIH_Server = ConfigurationManager.AppSettings["GPIH_Server"];
            GPIH_Database = ConfigurationManager.AppSettings["GPIH_Database"];
            GPIH_User = ConfigurationManager.AppSettings["GPIH_User"];

            var pass = ConfigurationManager.AppSettings["GPIH_Password"];
            if (pass.IsNotNullAndWhiteSpace())
                GPIH_Password = SymCryptoHelper.TripleDESDecrypt(pass, Salt, Key, IV);


            CRM_Server = ConfigurationManager.AppSettings["CRM_Server"];
            CRM_Database = ConfigurationManager.AppSettings["CRM_Database"];
            CRM_User = ConfigurationManager.AppSettings["CRM_User"];

            pass = ConfigurationManager.AppSettings["CRM_Password"];
            if (pass.IsNotNullAndWhiteSpace())
                CRM_Password = SymCryptoHelper.TripleDESDecrypt(pass, Salt, Key, IV);

            TakeCount = ConfigurationManager.AppSettings["TakeCount"].ToInt32();
            BuilderConnectionStrings();
        }
        public static void Save()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                Set(config, "CreatorID", CreatorID.ToNullableString());
                Set(config, "CoordinatorID", CoordinatorID.ToNullableString());
                Set(config, "BranchID", CoordinatorID.ToNullableString());
                Set(config, "DeclaratorID", DeclaratorID.ToNullableString());
                Set(config, "PaymentReceiverID", PaymentReceiverID.ToNullableString());
                Set(config, "PaymentReceiverBankAccountID", PaymentReceiverBankAccountID.ToNullableString());
                Set(config, "ProviderID", ProviderID.ToNullableString());
                Set(config, "AppointmentServiceProviders", string.Join(";", AppointmentServiceProviders));
                Set(config, "OurDiscountReal", OurDiscountReal.ToNullableString());


                Set(config, "GPIH_Server", GPIH_Server.IfNullEmpty());
                Set(config, "GPIH_Database", GPIH_Database.IfNullEmpty());
                Set(config, "GPIH_User", GPIH_User.IfNullEmpty());
                Set(config, "GPIH_Password", SymCryptoHelper.TripleDESEncrypt(GPIH_Password.IfNullEmpty(), Salt, Key, IV));

                Set(config, "CRM_Server", CRM_Server.IfNullEmpty());
                Set(config, "CRM_Database", CRM_Database.IfNullEmpty());
                Set(config, "CRM_User", CRM_User.IfNullEmpty());
                Set(config, "CRM_Password", SymCryptoHelper.TripleDESEncrypt(CRM_Password.IfNullEmpty(), Salt, Key, IV));

                Set(config, "TakeCount", TakeCount.ToNullableString());

                config.Save();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while saving configuration (see inner exception).", ex);
            }
        }

        private static void Set(Configuration config, string key, string value)
        {
            if (config.AppSettings.Settings[key] == null)
                config.AppSettings.Settings.Add(key, value);
            else
                config.AppSettings.Settings[key].Value = value;
        }


        public static string GPIHConnectionString;
        public static string CRMConnectionString;
        public static void BuilderConnectionStrings()
        {
            GPIHConnectionString = SqlConnectionStringHelper.GetConnectionString(GPIH_Server, GPIH_Database, GPIH_User, GPIH_Password);
            CRMConnectionString = SqlConnectionStringHelper.GetConnectionString(CRM_Server, CRM_Database, CRM_User, CRM_Password);
        }

    }
}