using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FRTTMO.PaymentGateway.Dto
{
    public class CoreCustomerProfileDto
    {
        public string FirstName
        {
            get;
            set;
        }

        public string MiddleName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        [Required]
        public string FullName
        {
            get;
            set;
        }

        public int Gender
        {
            get;
            set;
        }

        public string CardId
        {
            get;
            set;
        }

        [DataType(DataType.Date)]
        public string DateIdentifier
        {
            get;
            set;
        }

        [DataType(DataType.Date)]
        public string BirthDate
        {
            get;
            set;
        }

        public int? CustomerType
        {
            get;
            set;
        }

        public string TaxNumber
        {
            get;
            set;
        }

        [Required]
        [MaxLength(10)]
        [MinLength(10)]
        [RegularExpression("(84|0[3|5|7|8|9])+([0-9]{8})\\b", ErrorMessage = "Điện thoại không hợp lệ")]
        public string MobilePhone
        {
            get;
            set;
        }

        [DataType(DataType.EmailAddress)]
        public string Email
        {
            get;
            set;
        }

        public string Image
        {
            get;
            set;
        }
    }
    public class CoreCustomerWork
    {
        public string JobTitle
        {
            get;
            set;
        }

        public string Position
        {
            get;
            set;
        }

        public string Company
        {
            get;
            set;
        }

        public string Phone
        {
            get;
            set;
        }

        public string WorkAddress
        {
            get;
            set;
        }
    }
    public class CoreCustomerAddressDto
    {
        public string Address
        {
            get;
            set;
        }

        public string WardCode
        {
            get;
            set;
        }

        public string DistrictCode
        {
            get;
            set;
        }

        public string ProvinceCode
        {
            get;
            set;
        }

        public bool IsPrimary
        {
            get;
            set;
        }
    }
    public class CoreCustomerInputDto
    {
        public CoreCustomerProfileDto Profile
        {
            get;
            set;
        }

        public CoreCustomerWork Work
        {
            get;
            set;
        }

        public IEnumerable<CoreCustomerAddressDto> Address
        {
            get;
            set;
        }
    }

    //------------------------ ouput-----------------------
    public class CoreCustomerDto
    {
        public Guid CustomerId
        {
            get;
            set;
        }
        public CoreCustomerProfile Profile
        {
            get;
            set;
        }

        public CoreCustomerWork Work
        {
            get;
            set;
        }

        public IEnumerable<CoreCustomerAddress> Address
        {
            get;
            set;
        }
        public Dictionary<string, object> IncludeAttributes
        {
            get;
            set;
        }
    }
    public class CoreCustomerAddress
    {
        public string Address
        {
            get;
            set;
        }

        public string WardCode
        {
            get;
            set;
        }

        public string WardName
        {
            get;
            set;
        }

        public string DistrictCode
        {
            get;
            set;
        }

        public string DistrictName
        {
            get;
            set;
        }

        public string ProvinceCode
        {
            get;
            set;
        }

        public string ProvinceName
        {
            get;
            set;
        }

        public bool IsPrimary
        {
            get;
            set;
        }

        public Guid CustomerAddressId
        {
            get;
            set;
        }
    }
    public class CoreCustomerProfile
    {
        public string FirstName
        {
            get;
            set;
        }

        public string MiddleName
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string FullName
        {
            get;
            set;
        }

        public int Gender
        {
            get;
            set;
        }

        public string GenderName
        {
            get;
            set;
        }

        public string CardId
        {
            get;
            set;
        }
        public string DateIdentifier
        {
            get;
            set;
        }
        public string BirthDate
        {
            get;
            set;
        }

        public int? CustomerType
        {
            get;
            set;
        }

        public string CustomerTypeName
        {
            get;
            set;
        }

        public string TaxNumber
        {
            get;
            set;
        }
        public string MobilePhone
        {
            get;
            set;
        }
        public string Email
        {
            get;
            set;
        }

        public string Image
        {
            get;
            set;
        }
    }
    //---------------------------------
    public class SearchCoreCustomerDto
    {
        public string CardId
        {
            get;
            set;
        }

        public string MobilePhone
        {
            get;
            set;
        }
    }
}
