using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Sync.Model;
using Zek.Extensions;

namespace Sync.Win
{
    partial class SyncService : Zek.Scheduler.BaseSchedulerService
    {
        public SyncService()
        {
            InitializeComponent();

            EventLog.Log = "Application";
        }


        public bool TaskScheduler
        {
            get { return IsTaskScheduler; }
            set { IsTaskScheduler = value; }
        }

        /// <summary>
        /// ლიმიტისთვის (არ არის გადასახდელი)
        /// </summary>
        private const int DefaultCategoryID = 2;
        /// <summary>
        /// ანგარიშსწორება სამედიცინო დაწესებულებასთან
        /// </summary>
        private const int DefaultPaymentTypeID = 71002;
        /// <summary>
        /// ორიგინალი
        /// </summary>
        private const int DefaultVersionTypeID = 1;

        protected override void OnStart(string[] args)
        {
            if (IsWriteLine)
            {
                Console.WriteLine("Press 'I' to install as service");
                Console.WriteLine("Press 'U' to install as service");
                //Console.WriteLine("Press 'S' to start service");
            }
            base.OnStart(args);
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            AppConfig.Load();

            if (AppConfig.CreatorID.IsNullOrDefault())
            {
                if (IsWriteLine)
                {
                    Console.Write("CreatorID: ");
                    AppConfig.CreatorID = Console.ReadLine().ToInt32();
                }

                if (AppConfig.CreatorID.IsNullOrDefault())
                    throw new ConfigurationErrorsException("CreatorID is zero");
            }


            var isINSEmpty = false;
            if (AppConfig.INS_Server.IsNullOrWhiteSpace())
            {
                isINSEmpty = true;
                if (IsWriteLine)
                {
                    Console.Write("INS_Server: ");
                    AppConfig.INS_Server = Console.ReadLine();
                }
                if (AppConfig.INS_Server.IsNullOrWhiteSpace())
                    throw new ConfigurationErrorsException("INS_Server is null or white space");
            }

            if (AppConfig.INS_Database.IsNullOrWhiteSpace())
            {
                isINSEmpty = true;
                if (IsWriteLine)
                {
                    Console.Write("INS_Database: ");
                    AppConfig.INS_Database = Console.ReadLine();
                }

                if (AppConfig.INS_Database.IsNullOrWhiteSpace())
                    throw new ConfigurationErrorsException("INS_Database is null or white space");
            }

            if (isINSEmpty && AppConfig.INS_User.IsNullOrWhiteSpace())
            {
                if (IsWriteLine)
                {
                    Console.Write("INS_User: ");
                    AppConfig.INS_User = Console.ReadLine();

                    if (AppConfig.INS_User.IsNotNullAndWhiteSpace())
                    {
                        Console.Write("INS_Password: ");
                        AppConfig.INS_Password = Console.ReadLine();
                    }
                }
            }


            var isCRMEmpty = false;
            if (AppConfig.CRM_Server.IsNullOrWhiteSpace())
            {
                isCRMEmpty = true;
                if (IsWriteLine)
                {
                    Console.Write("CRM_Server: ");
                    AppConfig.CRM_Server = Console.ReadLine();
                }
                if (AppConfig.CRM_Server.IsNullOrWhiteSpace())
                    throw new ConfigurationErrorsException("CRM_Server is null or white space");
            }

            if (AppConfig.CRM_Database.IsNullOrWhiteSpace())
            {
                isCRMEmpty = true;
                if (IsWriteLine)
                {
                    Console.Write("CRM_Database: ");
                    AppConfig.CRM_Database = Console.ReadLine();
                }

                if (AppConfig.CRM_Database.IsNullOrWhiteSpace())
                    throw new ConfigurationErrorsException("CRM_Database is null or white space");
            }

            if (isCRMEmpty && AppConfig.CRM_User.IsNullOrWhiteSpace())
            {
                if (IsWriteLine)
                {
                    Console.Write("CRM_User: ");
                    AppConfig.CRM_User = Console.ReadLine();

                    if (AppConfig.CRM_User.IsNotNullAndWhiteSpace())
                    {
                        Console.Write("CRM_Password: ");
                        AppConfig.CRM_Password = Console.ReadLine();
                    }
                }
            }

            if (isINSEmpty || isCRMEmpty)
            {
                AppConfig.Save();
            }
        }

        protected override void OnReadKey(ConsoleKeyInfo cki)
        {
            switch (cki.Key)
            {
                case ConsoleKey.I:
                    Program.Install();
                    break;

                case ConsoleKey.U:
                    Program.Uninstall();
                    break;
            }


            base.OnReadKey(cki);
        }

        protected override void ExecuteStep()
        {
            ExecutePerformedService();
            ExecuteInvoiceStatus();

            if (IsConsole || IsTaskScheduler)
                Environment.Exit(1);
        }


        private void ExecuteInvoiceStatus()
        {
            var date = DateTime.Now.Date.AddMonths(DateTime.Now.Day < 5 ? -2 : -1);

            var invoiceNumber = GetClaimNumber(string.Empty, date);
            var invoiceNumberMed = GetClaimNumber("MED", date);
            using (var db = INSEntities.CreateInstance(AppConfig.INSConnectionString))
            {
                var passiveStatusID = DD_InsMedicalProviderInvoiceStatus.Passive.ToInt32();
                var invoice = db.InsMedicalProviderInvoices.FirstOrDefault(v => v.N == invoiceNumber && v.InvoiceStatusID == passiveStatusID);
                var invoiceMed = db.InsMedicalProviderInvoices.FirstOrDefault(v => v.N == invoiceNumberMed && v.InvoiceStatusID == passiveStatusID);

                if (invoice != null)
                {
                    var msg = string.Format("Changing InsMedicalProviderInvoice.InvoiceStatusID: {0}", invoice.N);
                    EventLog.WriteEntry(msg);
                    if (IsWriteLine)
                        Console.WriteLine(msg);
                    invoice.InvoiceStatusID = DD_InsMedicalProviderInvoiceStatus.Complete.ToInt32();
                }

                if (invoiceMed != null)
                {
                    var msg = string.Format("Changing InsMedicalProviderInvoice.InvoiceStatusID: {0}", invoiceMed.N);
                    EventLog.WriteEntry(msg);
                    if (IsWriteLine)
                        Console.WriteLine(msg);
                    invoiceMed.InvoiceStatusID = DD_InsMedicalProviderInvoiceStatus.Complete.ToInt32();
                }

                db.SaveChanges();
                if (invoice != null)
                {
                    var msg = string.Format("Done InsMedicalProviderInvoice.InvoiceStatusID: {0}", invoice.N);
                    EventLog.WriteEntry(msg);
                    if (IsWriteLine)
                        Console.WriteLine(msg);
                }

                if (invoiceMed != null)
                {
                    var msg = string.Format("Done InsMedicalProviderInvoice.InvoiceStatusID: {0}", invoiceMed.N);
                    EventLog.WriteEntry(msg);
                    if (IsWriteLine)
                        Console.WriteLine(msg);
                }

            }
        }

        private List<T_SyncINS> SP_T_SyncINS_Get()
        {
            using (var db = CRMEntities.CreateInstance(AppConfig.CRMConnectionString))
            {
                return db.SP_T_SyncINS_Get(MergeOption.NoTracking).ToList();
            }
        }
        private static string GetClaimNumber(string policyNumber, DateTime date)
        {
            return policyNumber.Contains("MED", StringComparison.CurrentCultureIgnoreCase) || policyNumber.Contains("MECA", StringComparison.CurrentCultureIgnoreCase)
                ? string.Format("CURMED {0}/{1}", date.Year, date.Month)
                : string.Format("CUR {0}/{1}", date.Year, date.Month);
        }

        private void T_SyncINSSet(T_SyncINS sync, int? INSID, Status status, string errorText, DateTime? syncDate)
        {
            sync.INSID = INSID;
            sync.StatusID = status.ToByte();
            sync.ErrorText = errorText;
            sync.SyncDate = syncDate;
        }
        private void ExecutePerformedService()
        {
            var toBeSyncedList = SP_T_SyncINS_Get();
            EventLog.WriteEntry(string.Format("Starting T_SyncINS (Count: {0})", toBeSyncedList.Count));
            if (IsWriteLine)
            {
                Console.WriteLine();
                Console.WriteLine("Starting T_SyncINS (Count: {0})", toBeSyncedList.Count);
            }

            var doneCount = 0;
            for (var i = 0; i < toBeSyncedList.Count; i++)
            {
                var sync = toBeSyncedList[i];
                if (IsWriteLine)
                    Console.WriteLine("   {0} / {1}      ID:{2}     CreateDate:{3:G}", i + 1, toBeSyncedList.Count, sync.ID, sync.CreateDate);

                if (sync.TryCount < int.MaxValue)
                    sync.TryCount++;

                using (var dbCrm = CRMEntities.CreateInstance(AppConfig.CRMConnectionString))
                {
                    dbCrm.T_SyncINS.Attach(sync);
                    try
                    {
                        using (var dbINS = INSEntities.CreateInstance(AppConfig.INSConnectionString))
                        {
                            var crm = dbCrm.ClinicPerformedServices.AsNoTracking().FirstOrDefault(s => s.ID == sync.ServiceID);
                            //როცა კურაციოში ჩანაწერი არსებობს (არ წაუშლიათ), მაშინ უნდა დაასინქრონიზოს.
                            if (crm != null)
                            {
                                //როცა ApplyNumber არის ცარიელი, მაშინ უნდა გადაახტეს
                                if (crm.ApplyNumber.IsNullOrWhiteSpace())
                                {
                                    T_SyncINSSet(sync, sync.INSID, Status.Synced, null, DateTime.Now);
                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }

                                if (crm.Price == 0m)
                                {
                                    T_SyncINSSet(sync, null, Status.Synced, "ZERO", DateTime.Now);
                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }

                                crm.ApplyNumber = crm.ApplyNumber.Trim();
                                var apptService = dbINS.InsMedicalAppointmentServices.AsNoTracking().FirstOrDefault(a => a.PrescriptionFormN == crm.ApplyNumber);
                                //თუ ვერ იპოვა მიმართვის ნომრით ჯიპიაის ბაზაში მაშინ უნდა გადაახტეს.
                                if (apptService == null)
                                {
                                    T_SyncINSSet(sync, sync.INSID, Status.Synced, "NOT FOUND", DateTime.Now);
                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }

                                if (apptService.StatusID == 82001) //passive
                                {
                                    T_SyncINSSet(sync, sync.INSID, Status.Synced, "PASSIVE", DateTime.Now);
                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }


                                if (!AppConfig.AppointmentServiceProviders.Contains(apptService.ProviderID)) //111
                                {
                                    T_SyncINSSet(sync, sync.INSID, Status.Synced, "NOT VALID PROVIDER", DateTime.Now);
                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }



                                var appointment = apptService.InsMedicalAppointment;
                                var caseServiceEx = dbINS.InsMedicalCaseServiceExes.AsNoTracking().FirstOrDefault(c => c.AppointmentServiceID == apptService.ID);
                                if (caseServiceEx != null)
                                {
                                    T_SyncINSSet(sync, caseServiceEx.ID, Status.Synced, "ALREADY EXISTS", DateTime.Now);

                                    dbCrm.SaveChanges();
                                    doneCount++;
                                    continue;
                                }

                                var endOfMonth = crm.Date.GetEndOfMonth().Date;
                                var claimNumber = GetClaimNumber(crm.PolicyN, crm.Date);

                                var invoice = dbINS.InsMedicalProviderInvoices.FirstOrDefault(v => v.N == claimNumber);
                                //.FirstOrDefault(v => v.ProviderID == AppConfig.ProviderID && v.ServicePeriodYear == endOfMonth.Year && v.ServicePeriod == endOfMonth.Month);
                                if (invoice == null)
                                {
                                    invoice = new InsMedicalProviderInvoice
                                    {
                                        CreatorID = AppConfig.CreatorID,
                                        ApproverID = null,
                                        EntryDate = endOfMonth,
                                        ProviderID = AppConfig.ProviderID,
                                        N = claimNumber,
                                        Date = endOfMonth,
                                        Amount = 0m,
                                        Comment = null,
                                        ServicePeriod = endOfMonth.Month,
                                        OperatorID = null,
                                        OperatorChangeDate = null,
                                        ApproveDate = null,
                                        ProgrammeTypeID = 1, //Private medical insurance
                                        InvoiceStatusID = DD_InsMedicalProviderInvoiceStatus.Passive.ToInt32(),
                                        ServicePeriodYear = endOfMonth.Year
                                    };
                                    dbINS.InsMedicalProviderInvoices.Add(invoice);
                                }

                                //ვქმნით ახალ ქეის სერვის და ქეისს (ერთი ერთთან არის კავშირი) 
                                caseServiceEx = new InsMedicalCaseServiceEx
                                {
                                    InsMedicalCaseEx = new InsMedicalCaseEx
                                    {
                                        CreatorID = AppConfig.CreatorID,
                                        EntryDate = endOfMonth,
                                        CoordinatorID = AppConfig.CoordinatorID,
                                        DeclarationID = null, //todo curmed-ის მაგივრად უნდა დაწეროს WL - InsMedicalDeclaration.N
                                        BranchID = AppConfig.BranchID,
                                        N = claimNumber,
                                        NotificationDate = endOfMonth,
                                        DeclaratorID = AppConfig.DeclaratorID,
                                        InsurantPolicyID = appointment.InsurantPolicyID,
                                        PatientPolicyID = appointment.PatientPolicyID,
                                        PatientID = appointment.PatientID,
                                        PaymentReceiverID = AppConfig.PaymentReceiverID,
                                        PaymentTypeID = DefaultPaymentTypeID,
                                        Invoice = string.Empty,
                                        PatientIsInsuredInOtherCompany = false,
                                        PatientOtherInsuranceCompany = string.Empty,
                                        OtherInsuranceTypeID = null,
                                        ICD10 = null,
                                        FullDiagnosisID = null,
                                        Diagnosis = string.Empty,
                                        Comment = string.Empty,
                                        PaymentReceiverBankAccountID = AppConfig.PaymentReceiverBankAccountID,
                                        //InvoiceID = null, //InsMedicalProviderInvoice
                                        InsMedicalProviderInvoice = invoice,

                                        ICD10_2 = null,
                                        ResponsiblePersonID = null,
                                        ResponsiblePersonChangeDate = null,
                                        GuaranteeLetterDeclarationID = null,
                                        VersionTypeID = DefaultVersionTypeID,
                                        OriginalCaseID = null,
                                        UID = Guid.NewGuid()
                                    },

                                    CreatorID = AppConfig.CreatorID,
                                    EntryDate = endOfMonth,
                                    ServiceStart = crm.Date,
                                    ServiceEnd = crm.Date,
                                    N = claimNumber,
                                    TypeID = apptService.TypeID,
                                    ProviderID = AppConfig.ProviderID,
                                    ServiceID = apptService.ServiceID,
                                    Name = apptService.Service,
                                    Coverage = apptService.Coverage,
                                    RealPrice = crm.Price,
                                    //RealCustomerPaid =
                                    OtherInsurantPaid = 0,
                                    DeductedAmount = 0,
                                    DeductionTypeID = null,
                                    OurDiscountReal = AppConfig.OurDiscountReal,
                                    //ToBePaidReal =//ქვემოთ ვანიჭებ მნიშვნელობას
                                    //ToBePaidReal =//ქვემოთ ვანიჭებ მნიშვნელობას
                                    Notes = "CRM ID = " + crm.ID,
                                    ClaimManagerID = null,
                                    ClaimManagerApproveDate = null,
                                    HeadApproverID = null,
                                    HeadApproveDate = null,
                                    AccountantID = null,
                                    AccountantApproveDate = null,
                                    ExclusionComment = null,
                                    PrescriptionFormN = crm.ApplyNumber,

                                    AppointmentServiceID = apptService.ID,
                                    //InsMedicalAppointmentService = apptService,

                                    GuaranteeLetterItemID = null,
                                    ActID = null,
                                    //DeclaredEstimatedLoss = null, //ქვემოთ ვანიჭებ მნიშვნელობას
                                    NCSPID = apptService.NCSPID,
                                    LabTestID = apptService.LabTestID,
                                    NCSPID2 = apptService.NCSPID2,
                                    LabTestID2 = apptService.LabTestID2,
                                    CategoryID = DefaultCategoryID,
                                    DeclarationServiceID = null,
                                    WasCreatedFromDeclaration = null,
                                    ProviderName = null,
                                    BedDays = null,
                                    ToothNumber = null,
                                    GuaranteeLetterDeclarationServiceID = null,
                                    OriginalPrice = null,
                                    MedicalClaimOperationID = null,
                                    //T_MedicalClaimOperation
                                    //T_MedicalClaimOperationVersion
                                    LastModifierID = null,
                                    OriginalCaseServiceID = null,
                                    StateLimitExceeded = null,
                                    IsNotDefaultCoverage = null,
                                    MedicalPlanCoverageID = null
                                };

                                caseServiceEx.RealCustomerPaid = caseServiceEx.RealPrice * ((100m - caseServiceEx.Coverage) / 100m);
                                caseServiceEx.ToBePaidReal = (caseServiceEx.RealPrice - caseServiceEx.RealCustomerPaid) * ((100m - caseServiceEx.OurDiscountReal) / 100m);
                                caseServiceEx.ToBePaidReal2 = caseServiceEx.ToBePaidReal;
                                caseServiceEx.DeclaredEstimatedLoss = caseServiceEx.ToBePaidReal;

                                dbINS.InsMedicalCaseServiceExes.Add(caseServiceEx);
                                dbINS.SaveChanges();

                                T_SyncINSSet(sync, caseServiceEx.ID, Status.Synced, "ADD", DateTime.Now);
                            }
                            else //როცა წაიშალა კურაციოში ჩანაწერი, უნდა განულდეს ჯიპიაის ქეისი.
                            {
                                var lastSyncID = dbCrm.T_SyncINS.Where(s => s.ServiceID == sync.ServiceID && s.INSID != null).Max(m => (int?)m.ID); //ვპოულობთ crm სერვისი რა ID-ით დასინქტრონიზდა.
                                if (lastSyncID == null) //თუ არ გადმოუტანია ჯიპიაის ქეისებში მაშინ არაფერი არ უნდა მოხდეს.
                                {
                                    T_SyncINSSet(sync, sync.INSID, Status.Synced, null, DateTime.Now);
                                }
                                else
                                {
                                    var lastSync = dbCrm.T_SyncINS.FirstOrDefault(s => s.ID == lastSyncID); //კიდევ ერთხელ შევამოწმოთ ბოლო სინქრონიზაცია არსებობს თუ არა.
                                    if (lastSync == null) //თუ არ გადმოუტანია ჯიპიაიში, არაფერიც არ უნდა მოხდეს.
                                    {
                                        T_SyncINSSet(sync, sync.INSID, Status.Synced, null, DateTime.Now);
                                    }
                                    else //თუ იპოვა დასინქრონიზებული ჩანაწერი მაშინ უნდა შევეცადოთ ჯიპიაის სოფტში წაშლას.
                                    {
                                        var caseServiceEx = dbINS.InsMedicalCaseServiceExes.FirstOrDefault(c => c.ID == lastSync.INSID);
                                        if (caseServiceEx != null) //თუ ვიპოვეთ ჯიპიაიში დასინქრონიზებული ჩანაწერი, ვნახოთ თუ ClaimManagerID ცარიელია და წავშალოთ. თუ არადა ერორი
                                        {
                                            if (caseServiceEx.ClaimManagerID != null)
                                                T_SyncINSSet(sync, caseServiceEx.ID, Status.Synced, "ClaimManagerID != NULL", DateTime.Now);
                                            else
                                            {
                                                if (caseServiceEx.InsMedicalCaseEx != null)
                                                    dbINS.InsMedicalCaseExes.Remove(caseServiceEx.InsMedicalCaseEx);
                                                dbINS.InsMedicalCaseServiceExes.Remove(caseServiceEx);
                                                dbINS.SaveChanges();
                                                T_SyncINSSet(sync, lastSync.INSID, Status.Synced, "DELETE", DateTime.Now);
                                            }
                                        }
                                        else
                                        {
                                            T_SyncINSSet(sync, sync.INSID, Status.Synced, null, DateTime.Now);
                                        }
                                    }
                                }


                            }

                            dbCrm.SaveChanges();
                            doneCount++;
                        }

                    }
                    catch (Exception ex)
                    {
                        sync.StatusID = (int)Status.Error;
                        dbCrm.SaveChanges();
                        LogException(new Exception(string.Format(@"Error while syncing T_SyncINS (ID: {0})", sync.ID), ex));
                        continue;
                    }
                }
            }

            EventLog.WriteEntry(string.Format("Done T_SyncINS (Count: {0}/{1})", doneCount, toBeSyncedList.Count));
            if (IsWriteLine)
            {
                Console.WriteLine("Done T_SyncINS (Count: {0}/{1})", doneCount, toBeSyncedList.Count);
            }

        }
    }
}
