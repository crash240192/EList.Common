namespace EList.Common.Support
{
    public enum ErrorCode : int
    {
        //default errors    000
        OK = 0,
        InternalError = 1,
        IsNullOrEmpty = 2,
        FormatError = 3,
        FileNotSpecified = 4,
        InvalidValue = 5,
        AccessError = 6,
        

        //authorization
        AuthorizationDataNotFound = 1001,
        AuthorizationDataInactive = 1002,
        AuthenticationError = 1003,
        InvalidActivationKey = 1004,
        ActivationAttemptsExceed = 1005,
        PasswordsDontMatch = 1006,
        NewAndOldPasswordsMatch = 1007,
        AuthorizationContactIsNotEmpty = 1008,
        TokenNotFound = 1009,

        //accounts        
        DublicateAccount = 2001,
        AccountNotFound = 2002,
        RegistrationForbiden = 2003,

        //notifications
        UserHasNoNecessaryContacts = 3001,
        UnableToNotifyUser = 3002,

        //persons
        PersonNotExists = 4000,
        InvalidFirstName = 4001,
        InvalidLastName = 4002,

        //subscriptions
        SubscriptionAlreadyExists = 5000,
        SubscriptionNotExists = 5001,

        //events metadata
        EventTypeNotFound = 6000,
        EventCategoryNotFound = 6001,
        EventNotFound = 6002,
        EventParametersNotFound = 6003,
        EventTemplateNotFound = 6004,

        //invitations
        InvitationNotFound = 7000,
        InvitationForbidden = 7001,

        //contacts
        ContactNotFound = 8000,

        //avatars
        AccountAvatarsHistoryIsEmpty = 9000,
        OrganizationAvatarsHistoryIsEmpty = 9001,

        //wallets
        TariffNotFound = 10001,
        TariffValidatorNotFound = 10002,
        WalletNotFound = 10003,
        TariffNotAssigned = 10004,
        PaymentValueMustBeOverZero = 10005,
        AccountWalletAlreadyExists = 10006,

        //organizations
        OrganizationNotFound = 11001,
        OrganizationMemberAlreadyExists = 11002, 
        OrganizationMemberNotFound = 11003,
        OrganizationLegalDataRequired = 11004,
        OrganizationPayoutDataRequired = 11005, 
        OrganizationPaymentRequired = 11006,
        OrganizationNotVerified = 11007,

        //participations
        EventIsFull = 12001,

        //events
        EventCancelled = 13001,
        EventNotExists = 13002,
        EventAccessDenied = 13003,
        InvalidAgeLimitValue = 13004,

        //conversations
        MessageNotFound = 14001,
        MessageReplied = 14002,

        //rating
        RatingItemNotFound = 15001,

        //ws
        NoActiveSocketConnections = 16001,

        //media
        AlbumNotFound = 17001,
        AddPhotosNotAllowed = 17002,
        AlbumItemNotFound = 17003,

        //agreement
        AgreementNotFound = 18001,
        UserMustBeAuthorized = 18002,
        AgreementDocumentNotFound = 18003,
        DocumentHeaderIsEmpty = 18004,
        DocumentIsEmpty = 18005,
        InvalidVersion = 18006
    }
}
