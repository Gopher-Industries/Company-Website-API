using Google.Cloud.Firestore;

namespace ProjectX.WebAPI.Models.Database.Authentication
{
    public struct UserRole
    {

        #region Defined Roles

        /// <summary>
        /// The highest clearance role
        /// </summary>
        public static UserRole Admin { get; } = new UserRole("Admin");

        /// <summary>
        /// A role with semi-clearance over a user
        /// </summary>
        public static UserRole Caregiver { get; } = new UserRole("Caregiver");

        /// <summary>
        /// A role that can only manage itself
        /// </summary>
        public static UserRole Patient { get; } = new UserRole("Patient");

        #endregion

        #region Constructor

        private UserRole(string RoleValue)
        {
            _roleValue = RoleValue;
        }

        public static UserRole FromString(string Role)
        {
            return (UserRole)Role;
        }

        #endregion

        #region Properties

        private readonly string _roleValue;

        #endregion

        #region Conversion Operators

        public static implicit operator string(UserRole Role) => Role._roleValue;

        public static implicit operator UserRole(string Role) => Role switch
        {
            var _ when string.Equals(Role, Admin, StringComparison.OrdinalIgnoreCase) => Admin,
            var _ when string.Equals(Role, Caregiver, StringComparison.OrdinalIgnoreCase) => Caregiver,
            var _ when string.Equals(Role, Patient, StringComparison.OrdinalIgnoreCase) => Patient,
            _ => throw new ArgumentException(null, nameof(Role)),
        };

        public override string ToString() => _roleValue;

        #endregion

    }

    public class UserRoleFirestoreConverter : IFirestoreConverter<UserRole>
    {
        public UserRole FromFirestore(object value)
        {
            return UserRole.FromString(value as string);
        }

        public object ToFirestore(UserRole value)
        {
            return value.ToString();
        }
    }

}
