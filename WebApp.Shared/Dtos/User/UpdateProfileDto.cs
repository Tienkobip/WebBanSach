namespace WebApp.Shared.Dtos.User
{
    // Component dùng: SuaThongTinCaNhan.razor (Customer)
    public record UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; } = null;
        public string? Address { get; set; } = null;
        public string? AvatarBase64 { get; set; } = null;
        public string? AvatarFileName { get; set; } = null;

        public UpdateProfileDto() { }

        public UpdateProfileDto(string fullName, DateTime? dateOfBirth, string? address, string? avatarBase64, string? avatarFileName)
        {
            FullName = fullName;
            DateOfBirth = dateOfBirth;
            Address = address;
            AvatarBase64 = avatarBase64;
            AvatarFileName = avatarFileName;
        }
    } 
}
