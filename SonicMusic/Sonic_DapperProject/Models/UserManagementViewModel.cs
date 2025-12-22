namespace Sonic_DapperProject.Models
{
    public class UserManagementViewModel
    {
        public List<UserDto> Users { get; set; }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchTerm { get; set; } 
    }

    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string CityName { get; set; }
        public string SubscriptionType { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
