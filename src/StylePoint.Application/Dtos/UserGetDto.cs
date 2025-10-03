﻿namespace StylePoint.Application.Dtos;

public class UserGetDto
{
    public long UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string? ProfileImgUrl { get; set; }
}
