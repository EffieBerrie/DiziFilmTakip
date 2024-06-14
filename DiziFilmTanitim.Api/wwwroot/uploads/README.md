# Upload Klasörü

Bu klasör film/dizi afişleri ve oyuncu/yönetmen fotoğrafları için kullanılır.

## Klasör Yapısı:
- `/afisler/` - Film ve dizi afişleri
- `/fotograflar/` - Oyuncu ve yönetmen fotoğrafları

## Desteklenen Formatlar:
- .jpg, .jpeg, .png, .webp
- Maksimum dosya boyutu: 5MB

## API Endpoints:
- POST /api/upload/film/{filmId}/afis
- POST /api/upload/dizi/{diziId}/afis
- POST /api/upload/oyuncu/{oyuncuId}/fotograf
- POST /api/upload/yonetmen/{yonetmenId}/fotograf