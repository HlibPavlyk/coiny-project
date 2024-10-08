﻿
using CoinyProject.Application.Dto.User;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Application.DTO.Album
{
    public class AlbumViewGetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Rate { get; set; }
        public AlbumStatus Status { get; set; }
        public UserNameGetDto Author { get; set; }
        public DateTime UpdatedAt { get; set; }
        public IEnumerable<string> ImagesUrls { get; set; } = new List<string>();
    }
}