using System.Security.Claims;

namespace ParkIRC.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string GetEmail(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.Email)?.Value;
        }

        public static string GetPhoneNumber(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst(ClaimTypes.MobilePhone)?.Value;
        }

        public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        {
            return principal?.FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public static bool IsInRole(this ClaimsPrincipal principal, string role)
        {
            return principal?.IsInRole(role) ?? false;
        }

        public static bool HasClaim(this ClaimsPrincipal principal, string type, string value)
        {
            return principal?.HasClaim(type, value) ?? false;
        }

        public static string GetClaimValue(this ClaimsPrincipal principal, string type)
        {
            return principal?.FindFirst(type)?.Value;
        }

        public static IEnumerable<Claim> GetClaims(this ClaimsPrincipal principal, string type)
        {
            return principal?.FindAll(type) ?? Enumerable.Empty<Claim>();
        }

        public static bool HasPermission(this ClaimsPrincipal principal, string permission)
        {
            return principal?.HasClaim("Permission", permission) ?? false;
        }

        public static IEnumerable<string> GetPermissions(this ClaimsPrincipal principal)
        {
            return principal?.FindAll("Permission").Select(c => c.Value) ?? Enumerable.Empty<string>();
        }

        public static bool HasAnyPermission(this ClaimsPrincipal principal, params string[] permissions)
        {
            return permissions.Any(p => principal.HasPermission(p));
        }

        public static bool HasAllPermissions(this ClaimsPrincipal principal, params string[] permissions)
        {
            return permissions.All(p => principal.HasPermission(p));
        }

        public static bool HasAnyRole(this ClaimsPrincipal principal, params string[] roles)
        {
            return roles.Any(r => principal.IsInRole(r));
        }

        public static bool HasAllRoles(this ClaimsPrincipal principal, params string[] roles)
        {
            return roles.All(r => principal.IsInRole(r));
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("Admin") ?? false;
        }

        public static bool IsOperator(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("Operator") ?? false;
        }

        public static bool IsSupervisor(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("Supervisor") ?? false;
        }

        public static bool IsManager(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("Manager") ?? false;
        }

        public static string GetOperatorId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                return string.Empty;
                
            // First try to get the NameIdentifier claim which typically contains the user ID
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
                return userId;
                
            // Fall back to the name claim if NameIdentifier is not available
            return principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        }

        public static bool IsStaff(this ClaimsPrincipal principal)
        {
            return principal?.IsInRole("Staff") ?? false;
        }

        public static string GetFullName(this ClaimsPrincipal principal)
        {
            var firstName = principal?.FindFirst("FirstName")?.Value;
            var lastName = principal?.FindFirst("LastName")?.Value;
            return $"{firstName} {lastName}".Trim();
        }

        public static string GetAvatar(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("Avatar")?.Value;
        }

        public static string GetDepartment(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("Department")?.Value;
        }

        public static string GetPosition(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("Position")?.Value;
        }

        public static DateTime? GetLastLoginDate(this ClaimsPrincipal principal)
        {
            var lastLoginDateStr = principal?.FindFirst("LastLoginDate")?.Value;
            return DateTime.TryParse(lastLoginDateStr, out var lastLoginDate) ? lastLoginDate : null;
        }

        public static string GetLastLoginIP(this ClaimsPrincipal principal)
        {
            return principal?.FindFirst("LastLoginIP")?.Value;
        }

        public static bool IsActive(this ClaimsPrincipal principal)
        {
            var isActiveStr = principal?.FindFirst("IsActive")?.Value;
            return bool.TryParse(isActiveStr, out var isActive) && isActive;
        }

        public static DateTime? GetAccountExpiryDate(this ClaimsPrincipal principal)
        {
            var expiryDateStr = principal?.FindFirst("AccountExpiryDate")?.Value;
            return DateTime.TryParse(expiryDateStr, out var expiryDate) ? expiryDate : null;
        }

        public static bool IsAccountExpired(this ClaimsPrincipal principal)
        {
            var expiryDate = principal.GetAccountExpiryDate();
            return expiryDate.HasValue && expiryDate.Value < DateTime.UtcNow;
        }

        public static bool IsAccountLocked(this ClaimsPrincipal principal)
        {
            var isLockedStr = principal?.FindFirst("IsAccountLocked")?.Value;
            return bool.TryParse(isLockedStr, out var isLocked) && isLocked;
        }

        public static int GetFailedLoginAttempts(this ClaimsPrincipal principal)
        {
            var attemptsStr = principal?.FindFirst("FailedLoginAttempts")?.Value;
            return int.TryParse(attemptsStr, out var attempts) ? attempts : 0;
        }

        public static DateTime? GetLockoutEndDate(this ClaimsPrincipal principal)
        {
            var lockoutEndStr = principal?.FindFirst("LockoutEndDate")?.Value;
            return DateTime.TryParse(lockoutEndStr, out var lockoutEnd) ? lockoutEnd : null;
        }

        public static bool IsLockedOut(this ClaimsPrincipal principal)
        {
            var lockoutEnd = principal.GetLockoutEndDate();
            return lockoutEnd.HasValue && lockoutEnd.Value > DateTime.UtcNow;
        }

        public static string FindFirstValue(this ClaimsPrincipal principal, string claimType)
        {
            if (principal == null)
                return null;
                
            var claim = principal.FindFirst(claimType);
            return claim?.Value;
        }
    }
} 