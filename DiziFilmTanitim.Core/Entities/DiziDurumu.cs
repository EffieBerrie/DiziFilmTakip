namespace DiziFilmTanitim.Core.Entities
{
    public enum DiziDurumu
    {
        Bilinmiyor = 0,
        Duyuruldu = 1, // Henüz başlamadı ama duyurusu yapıldı
        DevamEdiyor = 2,
        Tamamlandi = 3, // Bitti
        IptalEdildi = 4,
        AraVerdi = 5   // Sezon arası veya belirsiz bir ara
    }
}