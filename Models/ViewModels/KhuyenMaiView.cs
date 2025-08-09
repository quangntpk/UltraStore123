using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UltraStrore.Data;

namespace UltraStrore.Models.ViewModels
{
    public class KhuyenMaiView
    {
        public int ID { get; set; }
        public string TenKhuyenMai { get; set; }
        public DateOnly? NgayBatDau { get; set; }
        public DateOnly? NgayKetThuc { get; set; }
        public int? PercentChung { get; set; }
        public List<byte[]>? HinhAnh { get; set; }
        public List<ChiTietKhuyenMaiView>? DanhSachKhuyenMai { get; set; }
        public MoTaKhuyenMai? MoTa { get; set; }
    }

    public class MoTaKhuyenMaiCreateModel
    {
        public string? ID { get; set; }
        public string? IdMoTa { get; set; }
        public MoTaKhuyenMai? MoTa { get; set; }
    }

    public class MoTaKhuyenMai
    {
        public HeaderKhuyenMai Header { get; set; }
        [JsonPropertyName("Picture")]
        public List<PictureKhuyenMai>? Pictures { get; set; } 
        public List<TitleKhuyenMai>? Title { get; set; }
    }

    public class HeaderKhuyenMai
    {
        public string Title { get; set; }
    }

    public class PictureKhuyenMai
    {
        public string? Url { get; set; }
    }

    public class TitleKhuyenMai
    {
        public string Name { get; set; }
        public List<SubtitleKhuyenMai>? Subtitle { get; set; }
        public PictureKhuyenMai? Picture { get; set; }
    }

    public class SubtitleKhuyenMai
    {
        public string Name { get; set; }
        public DescriptionKhuyenMai? Description { get; set; }
        public PictureKhuyenMai? Picture { get; set; }
    }

    public class DescriptionKhuyenMai
    {
        public string? Content { get; set; }
    }
}