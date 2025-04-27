namespace baseApp.Dtos;

public record CreateUserDto(string Email, string Password, string FullName);
public record UpdateUserDto(string FullName);
public record UserDto(Guid Id, string Email, string FullName);

