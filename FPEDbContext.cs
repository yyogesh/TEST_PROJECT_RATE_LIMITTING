using Abp.IdentityServer4;
using Abp.Organizations;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SEB.FPE.Accounting;
using SEB.FPE.Accounting.Dtos;
using SEB.FPE.Accounting.Holiday;
using SEB.FPE.Adhoc;
using SEB.FPE.AnnualEnrollment;
using SEB.FPE.AnnualEnrollment.Dtos;
using SEB.FPE.Audit;
using SEB.FPE.Authorization.Api;
using SEB.FPE.Authorization.Delegation;
using SEB.FPE.Authorization.Roles;
using SEB.FPE.Authorization.Users;
using SEB.FPE.BackgroundJobs;
using SEB.FPE.Benefits;
using SEB.FPE.Brokers;
using SEB.FPE.Calculators;
using SEB.FPE.Cards;
using SEB.FPE.Chat;
using SEB.FPE.ClaimFunding;
using SEB.FPE.CommissionSetup;
using SEB.FPE.CommonMethods;
using SEB.FPE.Communications;
using SEB.FPE.Consent;
using SEB.FPE.CostCenter;
using SEB.FPE.Countries;
using SEB.FPE.DashboardCustomization.Dtos;
using SEB.FPE.DefaultPermission;
using SEB.FPE.Documents;
using SEB.FPE.Dto;
using SEB.FPE.Editions;
using SEB.FPE.EventProcessing;
using SEB.FPE.Exports;
using SEB.FPE.Exports.Dtos;
using SEB.FPE.Footer;
using SEB.FPE.Footer.Dtos;
using SEB.FPE.FPESettings;
using SEB.FPE.Friendships;
using SEB.FPE.Imports;
using SEB.FPE.LookUps;
using SEB.FPE.LookUps.Dtos;
using SEB.FPE.MemberDataChange;
using SEB.FPE.Members;
using SEB.FPE.Modules;
using SEB.FPE.Modules.Dtos;
using SEB.FPE.MultiTenancy;
using SEB.FPE.MultiTenancy.Accounting;
using SEB.FPE.MultiTenancy.Payments;
using SEB.FPE.Notifications;
using SEB.FPE.OneSpanESign;
using SEB.FPE.Optimization;
using SEB.FPE.Organizations;
using SEB.FPE.Organizations.Policy;
using SEB.FPE.PayrollSchedules;
using SEB.FPE.PensionAccounts;
using SEB.FPE.Persons;
using SEB.FPE.PowerBiReport;
using SEB.FPE.ProvinceStates;
using SEB.FPE.Reports;
using SEB.FPE.Reports.Dtos;
using SEB.FPE.ServiceBus;
using SEB.FPE.SpendingAccountProration;
using SEB.FPE.SSO;
using SEB.FPE.SSOAuthentication;
using SEB.FPE.StaticScreenContents;
using SEB.FPE.Storage;
using SEB.FPE.Template;
using SEB.FPE.TenantContactUs;
using SEB.FPE.TenantInfo;
using SEB.FPE.RateLimiting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SEB.FPE.EntityFrameworkCore
{
    public class FPEDbContext : AbpZeroDbContext<Tenant, Role, User, FPEDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<ClassPensionAccountTranslation> ClassPensionAccountTranslations { get; set; }

        public virtual DbSet<ClassPensionAccount> ClassPensionAccounts { get; set; }

        public virtual DbSet<CategoryPackage> CategoryPackages { get; set; }
        public virtual DbSet<CategoryPackageTranslation> CategoryPackageTranslations { get; set; }
        public virtual DbSet<CarrierTerminationCountRegister> CarrierTerminationCountRegisters { get; set; }
        public virtual DbSet<LibraryClassTenantRateTemp> LibraryClassTenantRateTemps { get; set; }
        public virtual DbSet<MemberReductionHistory> MemberReductionHistories { get; set; }
        public virtual DbSet<MemberTempJson> MemberTempJsons { get; set; }
        public virtual DbSet<MemberStatusAutomation> MemberStatusAutomations { get; set; }
        public virtual DbSet<MemberStatusAutomationClass> MemberStatusAutomationClasss { get; set; }
        public virtual DbSet<SalaryTypeTranslation> SalaryTypeTranslations { get; set; }
        public virtual DbSet<FpeContactOrganizationunits> FpeContactOrganizationunitses { get; set; }
        public virtual DbSet<VwMemberUserDetail> VwMemberUserDetails { get; set; }
        public virtual DbSet<VwRecentActivityReport> VwRecentActivityReport { get; set; }
        public virtual DbSet<VwUserRoleDetails> VwUserRoleDetails { get; set; }
        public virtual DbSet<FuturePendingMemberBenefitEnrollment> FuturePendingMemberBenefitEnrollment { get; set; }
        public virtual DbSet<FutureMemberBenefitEnrollment> FutureMemberBenefitEnrollment { get; set; }
        public virtual DbSet<FuturePendingDependentBenefitCoverage> FuturePendingDependentBenefitCoverage { get; set; }
        public virtual DbSet<FutureDependentBenefitCoverage> FutureDependentBenefitCoverage { get; set; }
        public virtual DbSet<FuturePendingMemberDependentCOB> FuturePendingMemberDependentCOB { get; set; }
        public virtual DbSet<FutureMemberDependentCOB> FutureMemberDependentCOB { get; set; }
        public virtual DbSet<FuturePendingMemberEOICoverage> FuturePendingMemberEOICoverage { get; set; }
        public virtual DbSet<FutureMemberEOICoverage> FutureMemberEOICoverage { get; set; }
        public virtual DbSet<FuturePendingMemberSpendingAccountEnrollment> FuturePendingMemberSpendingAccountEnrollment { get; set; }
        public virtual DbSet<FutureMemberSpendingAccountEnrollment> FutureMemberSpendingAccountEnrollment { get; set; }

        #region DBsets

        public virtual DbSet<DivisionClassMapping> DivisionClassMappings { get; set; }
        public virtual DbSet<PlanSponsorRetroCalculationDates> PlanSponsorRetroCalculationDateses { get; set; }
        public virtual DbSet<JobLog> JobLogs { get; set; }
        public virtual DbSet<BillingRegisterHistory> BillingRegisterHistorys { get; set; }
        public virtual DbSet<CarrierRegisterHistory> CarrierRegisterHistorys { get; set; }
        public virtual DbSet<PayrollRegisterHistory> PayrollRegisterHistorys { get; set; }
        public virtual DbSet<PayrollRegisterHistoryTranslation> PayrollRegisterHistoryTranslations { get; set; }
        public virtual DbSet<BillingRegisterHistoryTranslation> BillingRegisterHistoryTranslations { get; set; }
        public virtual DbSet<PlanponsorMemberClassStatus> PlansponsorMemberClassStatuses { get; set; }
        public virtual DbSet<OrganizationStatus> OrganizationStatuses { get; set; }
        public virtual DbSet<PlansponsorClassEventConfig> PlansponsorClassEventConfigs { get; set; }

        public virtual DbSet<PlansponsorClassEvent> PlansponsorClassEvents { get; set; }
        public virtual DbSet<PlansponsorClassTenantEvent> PlansponsorClassTenantEvents { get; set; }
        public virtual DbSet<PlansponsorClassTenantEventConfig> PlansponsorClassTenantEventConfigs { get; set; }

        public virtual DbSet<CarrierBenefitSetup> CarrierBenefitSetups { get; set; }

        public virtual DbSet<MemberEnrolmentWindow> MemberEnrolmentWindows { get; set; }

        public virtual DbSet<MemberEnrolmentWindowType> MemberEnrolmentWindowTypes { get; set; }

        public virtual DbSet<OrganizationText> OrganizationTexts { get; set; }

        public virtual DbSet<TenantTextType> TenantTextTypes { get; set; }

        public virtual DbSet<MemberAuditTag> MemberAuditTags { get; set; }
        public virtual DbSet<MemberAuditTagTranslation> MemberAuditTagTranslations { get; set; }
        public virtual DbSet<DocumentSetup> DocumentSetups { get; set; }
        public virtual DbSet<CarrierExtractLog> CarrierExtractLogs { get; set; }
        public virtual DbSet<PayRollExtractLog> PayRollExtractLogs { get; set; }
        public virtual DbSet<BankfileExtractLog> BankfileExtractLogs { get; set; }
        public virtual DbSet<CarrierCodeConfigurationMapping> CarrierCodeConfigurationMappings { get; set; }
        public virtual DbSet<CarrierFTPSettings> CarrierFTPSettings { get; set; }
        public virtual DbSet<CarrierCodeConfiguration> CarrierCodeConfigurations { get; set; }
        public virtual DbSet<ExportTransactionalMapping> ExportTransactionalMappings { get; set; }
        public virtual DbSet<BillingExtractLog> BillingExtractLogs { get; set; }
        public virtual DbSet<PayrollRegisterTranslation> PayrollRegisterTranslations { get; set; }

        public virtual DbSet<BillingRegisterTranslation> BillingRegisterTranslations { get; set; }
        public virtual DbSet<PendingMemberEOICoverage> PendingMemberEOICoverages { get; set; }

        public virtual DbSet<MemberEOICoverage> MemberEOICoverages { get; set; }
        public virtual DbSet<MemberIds> MemberIdses { get; set; }

        public virtual DbSet<MemberIdType> MemberIdTypes { get; set; }
        public virtual DbSet<ExportFormatting> ExportFormattings { get; set; }

        public virtual DbSet<BillingRegister> BillingRegisters { get; set; }

        public virtual DbSet<PayrollRegister> PayrollRegisters { get; set; }

        public virtual DbSet<MemberDependentCOBCoverage> MemberDependentCOBCoverages { get; set; }

        public virtual DbSet<CalculatorProfileCarrierMapping> CalculatorProfileCarrierMappings { get; set; }

        public virtual DbSet<PendingMemberDependentCOB> PendingMemberDependentCOBs { get; set; }

        public virtual DbSet<CarrierRegister> CarrierRegisters { get; set; }

        public virtual DbSet<MemberBenefitBeneficiaryAllocation> MemberBenefitBeneficiaryAllocations { get; set; }

        public virtual DbSet<MemberBenefitFlexCarryover> MemberBenefitFlexCarryovers { get; set; }

        public virtual DbSet<CalculatorProfilePlanSponsor> CalculatorProfilePlanSponsors { get; set; }

        public virtual DbSet<BillingGroupAssignment> BillingGroupAssignments { get; set; }
        public virtual DbSet<BillingGroupTranslation> BillingGroupTranslations { get; set; }
        public virtual DbSet<BillingGroup> BillingGroups { get; set; }
        public virtual DbSet<CalculatorScheduleStep> CalculatorScheduleSteps { get; set; }
        public virtual DbSet<CalculatorSchedule> CalculatorSchedules { get; set; }
        public virtual DbSet<CalculatorProfileStep> CalculatorProfileSteps { get; set; }
        public virtual DbSet<CalculatorStep> CalculatorSteps { get; set; }
        public virtual DbSet<CalculatorProfile> CalculatorProfiles { get; set; }

        public virtual DbSet<PendingMemberBenefitFlexAllocation> PendingMemberBenefitFlexAllocations { get; set; }

        public virtual DbSet<MemberBenefitFlexAllocation> MemberBenefitFlexAllocations { get; set; }

        public virtual DbSet<DependentBenefitCoverage> DependentBenefitCoverages { get; set; }

        public virtual DbSet<MemberBenefitOverrides> MemberBenefitOverrideses { get; set; }

        public virtual DbSet<PlanSponsorClassFlexCreditTranslation> MemberClassFlexCreditTranslations { get; set; }

        public virtual DbSet<MemberBenefitEnrollment> MemberBenefitEnrollments { get; set; }
        public virtual DbSet<MemberModuleEnrollment> MemberModuleEnrollments { get; set; }

        public virtual DbSet<PlanSponsorClassFlexCredit> PlanSponsorClassFlexCredits { get; set; }

        public virtual DbSet<PlansponsorClassJson> PlansponsorMemberClassJsons { get; set; }

        public virtual DbSet<BenefitTaxRates> BenefitTaxRateses { get; set; }

        public virtual DbSet<DivisionProvinceTaxType> DivisionProvinceTaxTypes { get; set; }

        public virtual DbSet<ProvincialTaxRatesTranslation> ProvincialTaxRatesTranslations { get; set; }

        public virtual DbSet<ProvincialTaxRates> ProvincialTaxRateses { get; set; }

        public virtual DbSet<TenantEffectiveDateMasterTranslation> TenantEffectiveDateMasterTranslations { get; set; }

        public virtual DbSet<TenantEffectiveDateMaster> TenantEffectiveDateMasters { get; set; }

        public virtual DbSet<BenefitClassSetting> BenefitClassSettings { get; set; }

        public virtual DbSet<PendingMemberBenefitEnrollment> PendingMemberBenefitEnrollments { get; set; }

        public virtual DbSet<BenefitSettingTenant> BenefitSettingTenants { get; set; }

        public virtual DbSet<PendingDependentBenefitCoverage> PendingDependentBenefitCoverages { get; set; }

        public virtual DbSet<BenefitSettingValueTranslation> BenefitSettingValueTranslations { get; set; }

        public virtual DbSet<BenefitSettingValue> BenefitSettingValues { get; set; }

        public virtual DbSet<ClassCategoryTranslation> ClassCategoryTranslations { get; set; }

        public virtual DbSet<ClassCategory> ClassCategories { get; set; }

        public virtual DbSet<LibraryBenefitClassCoverageTranslation> LibraryBenefitClassCoverageTranslations { get; set; }

        public virtual DbSet<LibraryBenefitClassInclusionJSON> LibraryBenefitClassInclusionJSONs { get; set; }

        public virtual DbSet<TenantClassCategoryTranslation> TenantClassCategoryTranslations { get; set; }
        public virtual DbSet<TenantClassCategory> TenantClassCategories { get; set; }
        public virtual DbSet<BenefitSettingTypeMasterTranslation> BenefitSettingTypeMasterTranslations { get; set; }

        public virtual DbSet<BenefitSettingTypeMaster> BenefitSettingTypeMasters { get; set; }

        public virtual DbSet<PlanSponsorClassTranslation> PlanSponsorClassTranslations { get; set; }
        public virtual DbSet<LibraryBenefitClassCoverageDetail> LibraryBenefitClassCoverageDetails { get; set; }
        public virtual DbSet<LibraryBenefitClassCoverage> LibraryBenefitClassCoverages { get; set; }
        public virtual DbSet<LibraryBenefitClassCoverage_JSON> LibraryBenefitClassCoverage_JSONs { get; set; }
        public virtual DbSet<LibraryBenefitClassInclusionTranslation> LibraryBenefitClassInclusionTranslations { get; set; }
        public virtual DbSet<LibraryBenefitClassRate> LibraryBenefitClassRates { get; set; }
        public virtual DbSet<LibraryBenefitTenentRate> LibraryBenefitTenentRates { get; set; }
        public virtual DbSet<LibraryBenefitClassInclusion> LibraryBenefitClassInclusions { get; set; }
        public virtual DbSet<LibraryBenefitClassInclusionDetail> LibraryBenefitClassInclusionDetails { get; set; }
        public virtual DbSet<LibraryBenefitTranslation> LibraryBenefitTranslations { get; set; }
        public virtual DbSet<LibraryBenefit> LibraryBenefits { get; set; }
        public virtual DbSet<LibraryBenefitTypeTranslation> LibraryBenefitTypeTranslations { get; set; }
        public virtual DbSet<LibraryBenefitType> LibraryBenefitTypes { get; set; }

        public virtual DbSet<FileRawData> FileRawDatas { get; set; }
        public virtual DbSet<FileRecordLog> FileRecordLogs { get; set; }
        public virtual DbSet<FileStaging> FileStagings { get; set; }
        public virtual DbSet<FileMappingTranslation> FileMappingTranslations { get; set; }
        public virtual DbSet<MemberSalary> MemberSalaries { get; set; }
        public virtual DbSet<SalaryType> SalaryTypes { get; set; }
        public virtual DbSet<FileMapping> FileMappings { get; set; }
        public virtual DbSet<AdminLog> AdminLogs { get; set; }

        public virtual DbSet<VwOrganizationDetail> VwOrganizationDetails { get; set; }
        public virtual DbSet<VwUserOrganizationDetail> VwUserOrganizationDetails { get; set; }

        public virtual DbSet<MemberRelationshipCharacteristic> MemberRelationshipCharacteristics { get; set; }
        public virtual DbSet<MemberDependent> MemberDependents { get; set; }
        public virtual DbSet<MemberDivision> MemberDivisions { get; set; }
        public virtual DbSet<CommunicationUserExclusion> CommunicationUserExclusions { get; set; }
        public virtual DbSet<MemberBenefitCharacteristic> MemberBenefitCharacteristics { get; set; }
        public virtual DbSet<EventType> EventTypes { get; set; }
        public virtual DbSet<EventCategoryType> EventCategoryTypes { get; set; }
        public virtual DbSet<CommunicationImplementationScheme> CommunicationImplementationSchemes { get; set; }
        public virtual DbSet<OrganizationContact> OrganizationContacts { get; set; }
        public virtual DbSet<OrganizationEmail> OrganizationEmails { get; set; }
        public virtual DbSet<OrganizationNote> OrganizationNotes { get; set; }
        public virtual DbSet<OrganizationPhone> OrganizationPhones { get; set; }
        public virtual DbSet<CommunicationMessageTranslation> CommunicationMessageTranslations { get; set; }
        public virtual DbSet<PlanSponsorClass> PlanSponsorClasses { get; set; }
        public virtual DbSet<CommunicationMessage> CommunicationMessages { get; set; }
        public virtual DbSet<ProvinceBilingualSetup> ProvinceBilingualSetups { get; set; }
        public virtual DbSet<PersonEmail> PersonEmails { get; set; }
        public virtual DbSet<Person> Persons { get; set; }
        public virtual DbSet<PersonNote> PersonNotes { get; set; }
        public virtual DbSet<PersonPhone> PersonPhones { get; set; }
        public virtual DbSet<PersonAddress> PersonAddresses { get; set; }
        public virtual DbSet<JobRun> JobRuns { get; set; }

        public virtual DbSet<CommunicationDistributionScheme> CommunicationDistributionSchemes { get; set; }
        public virtual DbSet<CommunicationMemberShipScheme> CommunicationMemberShipSchemes { get; set; }
        public virtual DbSet<CommunicationScheme> Communications { get; set; }
        public virtual DbSet<JobSchedule> JobSchedules { get; set; }
        public virtual DbSet<JobProfile> JobProfiles { get; set; }
        public virtual DbSet<OrgFinancial> Financials { get; set; }
        public virtual DbSet<ProvinceState> ProvinceStates { get; set; }
        public virtual DbSet<ProvinceStateTranslation> ProvinceStateTranslations { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<OrganizationAddress> OrganizationAddresses { get; set; }
        public virtual DbSet<FpeOrganizationUnit> FpeOrganizationUnits { get; set; }
        public virtual DbSet<OrganizationTranslation> OrganizationTranslations { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<LookupTranslation> LookupTranslations { get; set; }
        public virtual DbSet<LookUp> LookUps { get; set; }
        public virtual DbSet<RoleLookUp> RoleLookUps { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<MemberClassEnrollment> MembersClassEnrollment { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<Document> Documents { get; set; }
        public virtual DbSet<MemberClassDocument> MemberClassDocuments { get; set; }
        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public virtual DbSet<SubscriptionPaymentExtensionData> SubscriptionPaymentExtensionDatas { get; set; }

        public virtual DbSet<UserDelegation> UserDelegations { get; set; }

        public virtual DbSet<TenantSetting> TenantSettings { get; set; }
        public virtual DbSet<OrganizationSetting> OrganizationSettings { get; set; }
        public virtual DbSet<FPESetting> FPESettings { get; set; }
        public virtual DbSet<MemberAudit> MemberAudits { get; set; }
        public virtual DbSet<BankingInstitutions> BankingInstitutions { get; set; }

        public virtual DbSet<MemberActionRequired> MemberActionsRequired { get; set; }

        public virtual DbSet<RoleGroup> RoleGroup { get; set; }

        public virtual DbSet<MemberStatusAccessMapping> MemberStatusAccessMappings { get; set; }

        public virtual DbSet<RolesTranslation> RolesTranslations { get; set; }
        public virtual DbSet<MemberStatusAutomationList> MemberStatusAutomationLists { get; set; }

        public virtual DbSet<TaxableReportRegister> TaxableReportRegisters { get; set; }

        public virtual DbSet<TenantContactUsMaster> TenantContactUs { get; set; }
        public virtual DbSet<MemberHCSATaxableBenefitAmount> MemberHCSATaxableBenefitAmounts { get; set; }
        public virtual DbSet<EmailRegister> EmailRegisters { get; set; }
        public virtual DbSet<EmailRegisterDetail> EmailRegisterDetails { get; set; }
        public virtual DbSet<SMSRegister> SMSRegisters { get; set; }

        public virtual DbSet<PensionAccount> PensionAccounts { get; set; }

        public virtual DbSet<PensionAccountTranslation> PensionAccountTranslations { get; set; }

        public virtual DbSet<RetirementSubscription> RetirementSubscriptions { get; set; }
        public virtual DbSet<StatusBasedMemberPortalAccess> StatusBasedMemberPortalAccesses { get; set; }

        public virtual DbSet<PensionInfoText> PensionInfoText { get; set; }
        public virtual DbSet<PensionInfoTextTranslation> PensionInfoTextTranslation { get; set; }

        public virtual DbSet<PensionClassDocuments> PensionClassDocuments { get; set; }

        public virtual DbSet<ClassPensionAccountDetail> ClassPensionAccountDetails { get; set; }
        public virtual DbSet<CarrierCodeConfigurationMappingTranslation> CarrierCodeConfigurationMappingTranslation { get; set; }
        public virtual DbSet<OSSOconfiguration> SSOconfigurations { get; set; }
        public virtual DbSet<OSSOattributemapping> OSSOattributemappings { get; set; }
        public virtual DbSet<TenantTranslation> TenantTranslations { get; set; }
        public virtual DbSet<TenantAddress> TenantAddresses { get; set; }
        public virtual DbSet<TenantFinancial> TenantFinancials { get; set; }
        public virtual DbSet<TenantFinancialTranslation> TenantFinancialTranslations { get; set; }
        public virtual DbSet<CarrierConfigurationMaster> CarrierConfigurationMaster { get; set; }
        public virtual DbSet<CarrierConfigurationDetails> CarrierConfigurationDetails { get; set; }
        public virtual DbSet<CarrierConfigurationCodes> CarrierConfigurationCodes { get; set; }
        public virtual DbSet<VwCarrierCodeConfigurationSetup> VwCarrierCodeConfigurationSetup { get; set; }
        public virtual DbSet<VwCarrierCodeConfigurationSetupBasic> VwCarrierCodeConfigurationSetupBasic { get; set; }
        public virtual DbSet<PendingMemberSpendingAccountEnrollment> PendingMemberSpendingAccountEnrollment { get; set; }
        public virtual DbSet<MemberSpendingAccountEnrollment> MemberSpendingAccountEnrollment { get; set; }

        //Brokers
        public virtual DbSet<BrokerLicense> BrokerLicenses { get; set; }
        public virtual DbSet<Broker> Brokers { get; set; }
        public virtual DbSet<BrokerStatus> BrokerStatus { get; set; }
        public virtual DbSet<BrokerDivision> BrokerDivisions { get; set; }
        public virtual DbSet<BrokerLinking> BrokerLinking { get; set; }
        public virtual DbSet<PersonFinancial> PersonFinancial { get; set; }

        //PowerBiReport
        public virtual DbSet<ReportType> ReportType { get; set; }
        public virtual DbSet<ReportLibrary> ReportLibrary { get; set; }

        public virtual DbSet<ReportAccess> ReportAccesses { get; set; }

        public virtual DbSet<ReportParameterMaster> ReportParameterMasters { get; set; }

        public virtual DbSet<ReportParameter> ReportParameters { get; set; }

        public virtual DbSet<Module> Modules { get; set; }

        public virtual DbSet<ModuleDetail> ModuleDetails { get; set; }

        public virtual DbSet<ModuleBenefitMapping> ModuleBenefitMappings { get; set; }

        public virtual DbSet<ModuleTranslation> ModuleTranslations { get; set; }
        public virtual DbSet<ModuleCostShareDetails> ModuleCostShareDetails { get; set; }
        //public virtual DbSet<TempUploadDataTracking> TempUploadDataTrackings { get; set; }

        public virtual DbSet<PolicyMaster> PolicyMasters { get; set; }
        public virtual DbSet<PolicyDetails> PolicyDetails { get; set; }
        public virtual DbSet<PolicyTpaFeeDetails> PolicyTpaFeeDetails { get; set; }
        public virtual DbSet<PlansponsorPolicyMapping> PolicyPlansponsorMappings { get; set; }
        public virtual DbSet<PlansponsorPolicyTpaFeeDetails> PlansponsorPolicyTpaFeeDetails { get; set; }



        public virtual DbSet<ContactCentre> ContactCentres { get; set; }
        public virtual DbSet<ContactCentreTranslation> ContactCentreTranslations { get; set; }
        public virtual DbSet<ReplacementFieldsMaster> ReplacementFieldsMasters { get; set; }


        public virtual DbSet<EmailRegisterAssignment> EmailRegisterAssignments { get; set; }

        public virtual DbSet<AdminUserGuidMapping> AdminUserGuidMapping { get; set; }

        public virtual DbSet<StaticScreenContent> StaticScreenContents { get; set; }
        public virtual DbSet<StaticScreenContentTranslation> StaticScreenContentTranslations { get; set; }
        public virtual DbSet<TenantBankFileMaster> TenantBankFileMaster { get; set; }
        public virtual DbSet<TenantBankFileDetail> TenantBankFileDetail { get; set; }

        public virtual DbSet<AdminFeeTax> AdminFeeTaxation { get; set; }
        public virtual DbSet<AdminFeeTaxTranslation> AdminFeeTaxationTranslations { get; set; }

        //
        public virtual DbSet<PaymentMaster> PaymentMasters { get; set; }
        public virtual DbSet<PaymentBillingMapping> PaymentBillingMappings { get; set; }

        public virtual DbSet<UserGuideMaster> UserGuideMaster { get; set; }
        public virtual DbSet<HolidayMaster> HolidayMaster { get; set; }
        public virtual DbSet<HolidayMasterTranslation> HolidayMasterTranslation { get; set; }
        public virtual DbSet<MemberDataChangeMaster> MemberDataChangeMaster { get; set; }
        public virtual DbSet<MemberDataChangeDetails> MemberDataChangeDetails { get; set; }
        public virtual DbSet<FieldAuditMaster> FieldAuditMaster { get; set; }
        public virtual DbSet<AdhocCredits> AdhocCredits { get; set; }
        public virtual DbSet<AdhocCreditsTranslation> AdhocCreditsTranslation { get; set; }
        public virtual DbSet<AdhocCommunication> AdhocCommunications { get; set; }
        public virtual DbSet<AdhocCommunicationScheme> AdhocCommunicationScheme { get; set; }
        public virtual DbSet<AdhocCommunicationSchemeTranslation> AdhocCommunicationSchemeTranslation { get; set; }
        public virtual DbSet<MemberDataChangeRecordStatus> MemberDataChangeRecordStatus { get; set; }
        public virtual DbSet<DocumentDivision> DocumentDivisions { get; set; }
        public virtual DbSet<ClaimFundingMaster> ClaimFundingMasters { get; set; }
        public virtual DbSet<CommissionCalculationType> CommissionCalculationTypes { get; set; }
        public virtual DbSet<BrokerCommissionSharing> BrokerCommissionSharing { get; set; }
        public virtual DbSet<BrokerCommissionSharingDetail> BrokerCommissionSharingDetail { get; set; }


        public virtual DbSet<CommissionRate> CommissionRate { get; set; }
        public virtual DbSet<CommissionRateDetail> CommissionRateDetails { get; set; }

        public virtual DbSet<AdhocCommunicationOrganizationSelection> AdhocCommunicationOrganizationSelections { get; set; }
        public virtual DbSet<AdhocCommunicationRoleDetail> AdhocCommunicationRoleDetails { get; set; }

        public virtual DbSet<ApiKeysModel> ApiKeysModels { get; set; }
        public virtual DbSet<ApiKeysMapping> ApiKeysMappings { get; set; }

        //Cards
        public virtual DbSet<CardClientMapping> CardClientMappings { get; set; }
        //public virtual DbSet<CardDetails> CardDetailss { get; set; }
        //public virtual DbSet<CardConfig> CardConfigs { get; set; }
        public virtual DbSet<CardConfiguration> CardConfigurations { get; set; }
        public virtual DbSet<ContentSectionSetup> ContentSectionSetups { get; set; }
        public virtual DbSet<CardDetail> CardDetails { get; set; }
        public virtual DbSet<Translation> Translations { get; set; }
        public virtual DbSet<MasterCard> MasterCards { get; set; }
        public virtual DbSet<ContentSectionMaster> ContentSectionMasters { get; set; }
        public virtual DbSet<Content> Contents { get; set; }
        public virtual DbSet<CardAudit> CardAudits { get; set; }
        public virtual DbSet<CardPlansponsorMapping> CardPlansponsorMappings { get; set; }

        public virtual DbSet<PolicyCommissionDetail> PolicyCommissionDetails { get; set; }
        public virtual DbSet<BillingCommissionRegister> BillingCommissionRegister { get; set; }
        public virtual DbSet<AdvisorCommissionSharing> AdvisorCommissionSharing { get; set; }
        public virtual DbSet<BillingCommissionRegisterHistory> BillingCommissionRegisterHistory { get; set; }
        public virtual DbSet<AdvisorCommissionSharingHistory> AdvisorCommissionSharingHistory { get; set; }
        public virtual DbSet<SpendingAccountProrationMaster> SpendingAccountProrationMaster { get; set; }
        public virtual DbSet<CommissionExtractLog> CommissionExtractLog { get; set; }
        public virtual DbSet<CommissionPlanSponsorExtractLog> CommissionPlanSponsorExtractLog { get; set; }
        public virtual DbSet<ReminderNotificationRegister> ReminderNotificationRegister { get; set; }
        public virtual DbSet<PaymentMasterCommissionSharing> PaymentMasterCommissionSharing { get; set; }
        public virtual DbSet<PaymentMasterCommissionPlanSponsor> PaymentMasterCommissionPlanSponsor { get; set; }
        public virtual DbSet<ModuleInstruction> ModuleInstruction { get; set; }
        public virtual DbSet<MemberCostCenter> MemberCostCenter { get; set; }
        public virtual DbSet<BeneficiaryRejectionReasons> BeneficiaryRejectionReasons { get; set; }
        public virtual DbSet<PaymentMasterCarrier> PaymentMasterCarrier { get; set; }
        public virtual DbSet<VwEntityWiseUserDetail> VwEntityWiseUserDetails { get; set; }
        public virtual DbSet<CommissionRetroCalculationDate> CommissionRetroCalculationDates { get; set; }
        public virtual DbSet<Opt_MembersSearch> Opt_MembersSearch { get; set; }
        public virtual DbSet<PaymentBillingCarrierMapping> PaymentBillingCarrierMapping { get; set; }
        public virtual DbSet<PaymentBillingCommissionMapping> PaymentBillingCommissionMapping { get; set; }
        public virtual DbSet<MemberBenefitCoverageAndCost> MemberCoverageAndCost { get; set; }
        public virtual DbSet<BenefitGroupMapping> BenefitGroupMapping { get; set; }
        public virtual DbSet<ConsentText> ConsentTexts { get; set; }
        public virtual DbSet<ConsentTextTranslation> ConsentTextTranslations { get; set; }
        public virtual DbSet<OrganizationCostCenter> OrganizationCostCenters { get; set; }
        public virtual DbSet<SignedConsent> signedConsent { get; set; }
        public virtual DbSet<TenantControl> TenantControls { get; set; }
        public virtual DbSet<MemberDirectBillingInformation> MemberDirectBillingInfo { get; set; }
        public virtual DbSet<ClaimFundingExtractLog> ClaimFundingExtractLogs { get; set; }
        public virtual DbSet<Opt_NotificationsSearch> Opt_NotificationsSearch { get; set; }
        public virtual DbSet<MemberStatusApi> MemberStatusApi { get; set; }
        public virtual DbSet<MemberRelationshipTypeApi> MemberRelationshipTypeApi { get; set; }
        public virtual DbSet<MemberConfigDataHistory> MemberConfigDataHistory { get; set; }
        public virtual DbSet<MemberBillingExtractLog> MemberBillingExtractLogs { get; set; }

        public virtual DbSet<PaymentMasterMember> PaymentMasterMembers { get; set; }
        public virtual DbSet<PaymentBillingMappingMember> PaymentBillingMappingMembers { get; set; }

        public virtual DbSet<Opt_EmployementDTLEdit> Opt_EmployementDTLEdits { get; set; }

        public DbSet<BoolResult> BoolResults { get; set; }
        public DbSet<GetARSummaryForViewDto> GetARSummaryLists { get; set; }
        public DbSet<GetARCurrentDetails> GetARCurrentDetails { get; set; }
        public DbSet<GetARInvoiceDetails> GetARInvoiceDetails { get; set; }
        public DbSet<GetARPaymentDetails> GetARPaymentDetails { get; set; }
        public DbSet<DefaultPermissionMaster> DefaultPermissionMaster { get; set; }

        public DbSet<EnrollmentEventSetupMaster> EnrollmentEventSetupMaster { get; set; }

        public DbSet<EnrollmentEventClassMapping> EnrollmentEventClassMapping { get; set; }
        public virtual DbSet<Opt_MemberDtlLogin> Opt_MemberDtlLogin { get; set; }
        public DbSet<Opt_MemberDtlDBoardLogin> Opt_MemberDtlDBoardLogin { get; set; }
        public DbSet<TPAProcessingRequiredReminder> TPAProcessingRequiredReminder { get; set; }

        public DbSet<AEEventMemberMapping> AEEventMemberMapping { get; set; }
        public DbSet<AEEventClassMemberMapping> AEEventClassMemberMapping { get; set; }
        public DbSet<AEEventDivisionMemberMapping> AEEventDivisionMemberMapping { get; set; }
        public DbSet<GetAECheckListDto> AECheckList { get; set; }

        public virtual DbSet<ISSOLog> ISSOLogs { get; set; }
        public virtual DbSet<ClassRetirementContributionSetup> ClassRetirementContributionSetups { get; set; }
        public virtual DbSet<MemberResultDTO> MemberIdResultDTO { get; set; }
        public virtual DbSet<HandleCancelEventDto> HandleCancelEventDto { get; set; }
        public virtual DbSet<EligibleNEdefualtMembersDTO> EligibleNEdefualtMembersDTO { get; set; }
        public virtual DbSet<EligibleAEdefualtMembersDTO> EligibleAEdefualtMembersDTO { get; set; }
        public virtual DbSet<StoredProcedureResponseDto> StoredProcedureResponseDto { get; set; }
        public virtual DbSet<AEReminderCommunicationsDTO> AEReminderCommunicationsDto { get; set; }
        public virtual DbSet<GetAllClassTypeDto> GetAllClassTypeDto { get; set; }
        public virtual DbSet<fpe_releases> fpe_releases { get; set; }
        public virtual DbSet<fpe_filetemplate> fpe_filetemplates { get; set; }
        public virtual DbSet<fpe_filetemplatetranslation> fpe_filetemplatetranslations { get; set; }
        //public virtual DbSet<fpe_filetemplateLocation> fpe_filetemplateLocations { get; set; }
        public virtual DbSet<fpe_filetemplatemapping> fpe_filetemplatemappings { get; set; }
        public virtual DbSet<fpe_HRISfieldmapping> fpe_HRISfieldmappings { get; set; }
        public virtual DbSet<fpe_filetemplateDetails> fpe_filetemplateDetail { get; set; }
        public virtual DbSet<fpe_filetemplateProperty> fpe_filetemplatePropertys { get; set; }
        public virtual DbSet<fpe_filelogimport> fpe_filelogimports { get; set; }
        public virtual DbSet<fpe_filelogimportprogress> fpe_filelogimportprogress { get; set; }
        public virtual DbSet<fpe_hrisstatuscode> fpe_hrisstatuscodes { get; set; }
        public virtual DbSet<fpe_HRISStatusCode_Translation> fpe_HRISStatusCode_Translations { get; set; }
        public virtual DbSet<fpe_HRISstaging> fpe_HRISstagings { get; set; }
        public virtual DbSet<fpe_HRISStagingDataAudit> fpe_HRISStagingDataAudits { get; set; }
        public virtual DbSet<fpe_HRISfieldmapping_Translation> fpe_HRISfieldmapping_Translations { get; set; }
        public virtual DbSet<CommissionPaymentSetting> commissionPaymentSettings { get; set; }
        public virtual DbSet<MemberEligibilityDateChangeHistory> MemberEligibilityDateChangeHistorys { get; set; }
        public virtual DbSet<RecalculateMemberBenefits> RecalculateMemberBenefits { get; set; }

        public DbSet<PayrollExportDetailsDTO> PayrollExportDetailsDTOs { get; set; }
        public DbSet<PayrollResultStatsDTO> PayrollResultStatsDTOs { get; set; }
        public DbSet<PayrollErroredDataDTO> PayrollErroredDataDTOs { get; set; }
        public DbSet<GetVwRecentActivityReportViewDto> GetVwRecentActivityReportViewDto { get; set; }
        public virtual DbSet<fpe_HRISRecalc> fpe_HRISRecalc { get; set; }
        public virtual DbSet<MemberESignature> MemberESignatures { get; set; }
        public virtual DbSet<BillSettlement> BillSettlements { get; set; }
        public virtual DbSet<MemberAttestationSelection> MemberAttestationSelections { get; set; }
        public DbSet<fpe_azureservicebuslog> fpe_azureservicebuslog { get; set; }

        public virtual DbSet<PayrollRetroCalculationDates> PayrollRetroCalculationDates { get; set; }
        public virtual DbSet<PayrollSchedule> PayrollSchedules { get; set; }
        public virtual DbSet<PayrollScheduleMapping> PayrollScheduleMappings { get; set; }
        public virtual DbSet<PayrollScheduleType> PayrollScheduleTypes { get; set; }
        public virtual DbSet<PayrollExtractFileDto> PayrollExtractFileDto { get; set; }
        public virtual DbSet<PlanponsorClassCalculatorProfileMapping> PlanponsorClassCalculatorProfileMappings { get; set; }
        public virtual DbSet<BankfileSequenceNumberTracking> BankfileSequenceNumberTracking { get; set; }
        public virtual DbSet<SSOConfigurationSetting> SSOConfigurationSettings { get; set; }
        public virtual DbSet<CanDeleteModuleDto> CanDeleteModuleDto { get; set; }
        public DbSet<fpe_payrollfiletemplate> PayrollFileTemplateRepository { get; set; }
        public DbSet<fpe_payrollfiletemplate_Translation> PayrollFileTranslationTemplateRepository { get; set; }
        public DbSet<fpe_payrollrecordtypecharacteristic> PayrollRecordTypeCharacteristicRepository { get; set; }
        public DbSet<fpe_payrollfiletemplateproperty> PayrollFileTranslationTemplatePropertyRepository { get; set; }
        public DbSet<fpe_payrollfiletemplatedetails> PayrollFileFieldDetailsRepository { get; set; }
        public DbSet<fpe_payrollfieldcondition> PayrollFileFieldConditionRepository { get; set; }
        public DbSet<fpe_payrollfileformattingrule> PayrollFileFormattingRuleRepository { get; set; }
        public virtual DbSet<RateLimitEntry> RateLimitEntries { get; set; }

        public FPEDbContext(DbContextOptions<FPEDbContext> options)
            : base(options)
        {

        }
        #endregion

        #region ModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ClassPensionAccountTranslation>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ClassPensionAccount>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CarrierTerminationCountRegister>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<MemberStatusAutomation>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberStatusAutomationClass>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<SalaryTypeTranslation>(s =>
                       {
                           s.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FpeContactOrganizationunits>(f =>
                       {
                           f.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<VwMemberUserDetail>(v =>
                       {
                           v.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<DivisionClassMapping>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<PlanSponsorRetroCalculationDates>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BillingRegisterHistory>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<CarrierRegisterHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<PayrollRegisterHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<PayrollRegisterHistoryTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<BillingRegisterHistoryTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PlanponsorMemberClassStatus>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<OrganizationStatus>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PlansponsorClassEventConfig>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlansponsorClassEvent>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<PlansponsorClassTenantEvent>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PlansponsorClassTenantEventConfig>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberEnrolmentWindow>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberEnrolmentWindowType>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CarrierBenefitSetup>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrganizationText>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberAuditTag>(d =>
                       {
                           d.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberAuditTagTranslation>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<DocumentSetup>(d =>
            {
                d.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CarrierExtractLog>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BankfileExtractLog>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CarrierCodeConfigurationMapping>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CarrierCodeConfiguration>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<ExportTransactionalMapping>(x =>
                       {
                           x.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BillingExtractLog>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<OrganizationSetting>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantSetting>(t =>
                       {
                           t.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PayrollRegisterTranslation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BillingRegisterTranslation>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberIds>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberIdType>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<PendingMemberEOICoverage>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberEOICoverage>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ExportFormatting>(x =>
                       {
                           x.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BillingRegister>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PayrollRegister>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberDependentCOBCoverage>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CarrierRegister>(c =>
                                  {
                                      c.HasIndex(e => new { e.TenantId });
                                  });
            modelBuilder.Entity<MemberBenefitBeneficiaryAllocation>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberBenefitFlexCarryover>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CalculatorProfilePlanSponsor>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BillingGroupAssignment>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BillingGroupTranslation>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BillingGroup>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CalculatorProfile>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<CalculatorProfileStep>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CalculatorScheduleStep>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CalculatorSchedule>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CalculatorStep>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<VwUserOrganizationDetail>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<VwOrganizationDetail>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<PendingMemberBenefitFlexAllocation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberBenefitFlexAllocation>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<DependentBenefitCoverage>(d =>
                       {
                           d.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberBenefitOverrides>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlanSponsorClassFlexCreditTranslation>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberBenefitEnrollment>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberModuleEnrollment>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PlanSponsorClassFlexCredit>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlansponsorClassJson>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BenefitTaxRates>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<DivisionProvinceTaxType>(d =>
                       {
                           d.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<ProvincialTaxRatesTranslation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ProvincialTaxRates>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantEffectiveDateMasterTranslation>(t =>
                       {
                           t.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<TenantEffectiveDateMaster>(t =>
                       {
                           t.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BenefitClassSetting>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PendingMemberBenefitEnrollment>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BenefitSettingTenant>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PendingDependentBenefitCoverage>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BenefitSettingValueTranslation>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<BenefitSettingValue>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<ClassCategoryTranslation>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ClassCategory>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<LibraryBenefitClassCoverageTranslation>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitClassInclusionJSON>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<TenantClassCategoryTranslation>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<Document>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberClassDocument>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantClassCategory>(t =>
            {
                t.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BenefitSettingTypeMasterTranslation>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BenefitSettingTypeMaster>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlanSponsorClassTranslation>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitClassCoverage>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<LibraryBenefitClassCoverageDetail>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<LibraryBenefitClassCoverage_JSON>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitClassRate>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                           l.Property(e => e.IsTenentRate).HasConversion<bool?>(f => f, t => t ?? false);
                       });
            modelBuilder.Entity<LibraryBenefitTenentRate>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<LibraryBenefitClassCoverageDetail>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitClassInclusion>(l =>

                       modelBuilder.Entity<LibraryBenefitClassInclusionTranslation>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       }));
            modelBuilder.Entity<LibraryBenefitClassInclusion>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitTranslation>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefit>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                           l.Property(e => e.IsTaxableBenefit).HasConversion<bool?>(f => f, t => t ?? false);
                           l.Property(e => e.IsNEMrequired).HasConversion<bool?>(f => f, t => t ?? false);
                       });
            modelBuilder.Entity<LibraryBenefitTypeTranslation>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LibraryBenefitType>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FileRawData>(f =>
                       {
                           f.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FileRecordLog>(f =>
                       {
                           f.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FileStaging>(f =>
                       {
                           f.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FileMappingTranslation>(f =>
                       {
                           f.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<FileRawData>(f =>
            {
                f.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberSalary>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<SalaryType>(s =>
            {
                s.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<FileMapping>(f =>
            {
                f.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<AdminLog>(a =>
                       {
                           a.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberRelationshipCharacteristic>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<MemberDependent>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<MemberDivision>(m =>
            {
                m.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CommunicationUserExclusion>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberBenefitCharacteristic>(m =>
                       {
                           m.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<EventType>(x =>
            {
                x.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<EventCategoryType>(x =>
                       {
                           x.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CommunicationImplementationScheme>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<Member>(o =>
                    {
                        o.HasIndex(e => new { e.TenantId });
                    });
            modelBuilder.Entity<MemberClassEnrollment>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrganizationContact>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrganizationEmail>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<OrganizationPhone>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PlanSponsorClass>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<CommunicationMessage>(c =>
                       {
                           c.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PersonEmail>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<Person>(p =>
                       {
                           p.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<JobRun>(j =>
                       {
                           j.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<OrganizationNote>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrgFinancial>(f =>
            {
                f.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CommunicationMessageTranslation>(c =>
         {
             c.HasIndex(e => new { e.TenantId });
         });

            modelBuilder.Entity<CommunicationScheme>(c =>
            {
                c.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<JobSchedule>(j =>
            {
                j.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<JobProfile>(j =>
                       {
                           j.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<OrganizationAddress>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<PersonNote>(p =>
                      {
                          p.HasIndex(e => new { e.TenantId });
                      });

            modelBuilder.Entity<PersonPhone>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrganizationTranslation>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<Organization>(o =>
                       {
                           o.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LookupTranslation>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<LookUp>(l =>
                       {
                           l.HasIndex(e => new { e.TenantId });
                       });
            modelBuilder.Entity<RoleLookUp>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BinaryObject>(b =>
                       {
                           b.HasIndex(e => new { e.TenantId });
                       });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
            });

            modelBuilder.Entity<SubscriptionPaymentExtensionData>(b =>
            {
                b.HasQueryFilter(m => !m.IsDeleted)
                    .HasIndex(e => new { e.SubscriptionPaymentId, e.Key, e.IsDeleted })
                    .IsUnique();
            });

            modelBuilder.Entity<UserDelegation>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.SourceUserId });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId });
            });

            //Lookup Navigation
            modelBuilder.Entity<LookUp>(b => b.HasOne(e => e.Parent).WithMany(x => x.Children).HasForeignKey(g => g.ParentTableId));

            modelBuilder.Entity<TenantSetting>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<OrganizationSetting>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberAudit>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<JobLog>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
                l.Property(e => e.IsProcessed).HasConversion<bool?>(f => f, t => t ?? false);
            });

            modelBuilder.Entity<MemberStatusAccessMapping>(l =>
            {
                l.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<RolesTranslation>(s =>
            {
                s.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberStatusAutomationList>(s =>
            {
                s.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<TaxableReportRegister>(s =>
            {
                s.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberHCSATaxableBenefitAmount>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<RetirementSubscription>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PensionInfoText>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PensionInfoTextTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ClassPensionAccountDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CarrierCodeConfigurationMappingTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OSSOconfiguration>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OSSOattributemapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantAddress>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantFinancial>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantFinancialTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<CarrierConfigurationMaster>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CarrierConfigurationDetails>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CarrierConfigurationCodes>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<VwCarrierCodeConfigurationSetup>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<VwCarrierCodeConfigurationSetupBasic>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BrokerLinking>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<PendingMemberSpendingAccountEnrollment>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<MemberSpendingAccountEnrollment>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<Broker>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BrokerStatus>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PersonFinancial>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<Module>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ModuleDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ModuleBenefitMapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ModuleTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            //modelBuilder.Entity<TempUploadDataTracking>(o =>
            //{
            //    o.HasIndex(e => new { e.TenantId });
            //});

            modelBuilder.Entity<ContactCentre>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ContactCentreTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ReplacementFieldsMaster>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<StaticScreenContent>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<StaticScreenContentTranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantBankFileMaster>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<TenantBankFileDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<AdminFeeTaxTranslation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<AdminFeeTax>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<UserGuideMaster>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<HolidayMaster>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<HolidayMasterTranslation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberDataChangeMaster>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberDataChangeDetails>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<FieldAuditMaster>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberDataChangeRecordStatus>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<AdhocCredits>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<AdhocCreditsTranslation>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ClaimFundingMaster>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CommissionCalculationType>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BrokerCommissionSharing>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BrokerCommissionSharingDetail>(p =>
            {
                p.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CommissionRate>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CommissionRateDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CardClientMapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            //modelBuilder.Entity<CardDetails>(o =>
            //{
            //    o.HasIndex(e => new { e.TenantId });
            //});
            //modelBuilder.Entity<CardConfig>(o =>
            //{
            //    o.HasIndex(e => new { e.TenantId });
            //});
            modelBuilder.Entity<CardConfiguration>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CardDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ContentSectionSetup>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<Translation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MasterCard>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<Content>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ContentSectionMaster>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CardAudit>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CardPlansponsorMapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ApiKeysModel>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ApiKeysMapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();
            modelBuilder.Entity<PolicyCommissionDetail>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BillingCommissionRegister>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();
            modelBuilder.Entity<AdvisorCommissionSharing>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();

            modelBuilder.Entity<BillingCommissionRegisterHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();
            modelBuilder.Entity<AdvisorCommissionSharingHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();

            modelBuilder.Entity<ReminderNotificationRegister>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PaymentMasterCommissionSharing>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<PaymentMasterCommissionPlanSponsor>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();

            modelBuilder.Entity<MemberCostCenter>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BeneficiaryRejectionReasons>(o =>
            {
            });
            modelBuilder.Entity<CommissionRetroCalculationDate>(o =>
            {
            });
            modelBuilder.Entity<MemberBenefitCoverageAndCost>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<OrganizationCostCenter>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberConfigDataHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberBillingExtractLog>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<EnrollmentEventSetupMaster>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<EnrollmentEventClassMapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ISSOLog>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<ClassRetirementContributionSetup>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberEligibilityDateChangeHistory>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<RecalculateMemberBenefits>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_releases>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filetemplate>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filetemplatetranslation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            //modelBuilder.Entity<fpe_filetemplateLocation>(o =>
            //{
            //    o.HasIndex(e => new { e.TenantId });
            //});
            modelBuilder.Entity<fpe_filetemplatemapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISfieldmapping>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filetemplateDetails>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filetemplateProperty>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filelogimport>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_filelogimportprogress>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_hrisstatuscode>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISStatusCode_Translation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISstaging>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISStagingDataAudit>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISfieldmapping_Translation>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<CommissionPaymentSetting>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<FuturePendingMemberBenefitEnrollment>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<FutureMemberBenefitEnrollment>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FuturePendingDependentBenefitCoverage>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FutureDependentBenefitCoverage>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FuturePendingMemberDependentCOB>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FutureMemberDependentCOB>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FuturePendingMemberEOICoverage>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FutureMemberEOICoverage>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FuturePendingMemberSpendingAccountEnrollment>(o =>
        {
            o.HasIndex(e => new { e.TenantId });
        });
            modelBuilder.Entity<FutureMemberSpendingAccountEnrollment>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_HRISRecalc>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberESignature>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<MemberAttestationSelection>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<fpe_azureservicebuslog>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<BankfileSequenceNumberTracking>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.Entity<SSOConfigurationSetting>(o =>
            {
                o.HasIndex(e => new { e.TenantId });
            });
            modelBuilder.ConfigurePersistedGrantEntity();

            modelBuilder.Entity<BoolResult>().HasNoKey();
            modelBuilder.Entity<GetARSummaryForViewDto>().HasNoKey();
            modelBuilder.Entity<GetARCurrentDetails>().HasNoKey();
            modelBuilder.Entity<GetARInvoiceDetails>().HasNoKey();
            modelBuilder.Entity<GetARPaymentDetails>().HasNoKey();
            modelBuilder.Entity<TPAProcessingRequiredReminder>().HasNoKey();
            modelBuilder.Entity<AEEventMemberMapping>().HasNoKey();
            modelBuilder.Entity<AEEventClassMemberMapping>().HasNoKey();
            modelBuilder.Entity<AEEventDivisionMemberMapping>().HasNoKey();
            modelBuilder.Entity<MemberResultDTO>().HasNoKey();
            modelBuilder.Entity<HandleCancelEventDto>().HasNoKey();
            modelBuilder.Entity<EligibleNEdefualtMembersDTO>().HasNoKey();
            modelBuilder.Entity<EligibleAEdefualtMembersDTO>().HasNoKey();
            modelBuilder.Entity<StoredProcedureResponseDto>().HasNoKey();
            modelBuilder.Entity<GetAllClassTypeDto>().HasNoKey();
            
            modelBuilder.Entity<AEReminderCommunicationsDTO>().HasNoKey();
            modelBuilder.Entity<GetAECheckListDto>().HasNoKey();
            modelBuilder.Entity<PayrollExtractFileDto>().HasNoKey();
            modelBuilder.Entity<CanDeleteModuleDto>().HasNoKey();

			 foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			 {
				 foreach (var property in entityType.GetProperties())
				 {
					 if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
					 {
						 property.SetPrecision(18);
						 property.SetScale(4);
					 }
				 }
			 }
 
        }
        #endregion

    }

}
