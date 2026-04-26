using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        //accounts
        DublicaeAccount = 2001,
        AccountNotFound = 2002,

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

        //invitations
        InvitationNotFound = 7000,

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
        OrganizationNotFound = 11001
    }
}
