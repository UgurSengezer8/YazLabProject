# KARANTINA-13 — BÖLÜM 01 TAM PAKET

Bu paket 1 adet tam bölüm tasarımı için hazırlanmıştır.

## İçerik
- 1 tam bölüm haritası (Map/level01_complete_map.png)
- Oyuncu sprite sheet + yön bazlı tekil frame dosyaları
- Robot sprite sheet + yön bazlı tekil frame dosyaları
- Zemin, duvar, kapı ve prop sprite’ları
- Elektrik bariyeri animasyonu
- Sürgülü kapı açılma animasyonu
- Evidence terminal animasyonu
- ARK-13 çekirdek animasyonu
- Ön izleme dosyaları ve manifest JSON

## Klasörler
- Assets/Karantina13/Characters/Player
- Assets/Karantina13/Characters/Robot
- Assets/Karantina13/Tiles
- Assets/Karantina13/Props
- Assets/Karantina13/Animations
- Assets/Karantina13/Map
- Docs/asset_manifest.json

## Oyuncu Yönleri
Satırlar şu sırayla hazırlanmıştır:
1. down
2. left
3. right
4. up

Oyuncu kare adları:
- idle
- step1
- pass1
- step2
- pass2

## Robot Yönleri
Satırlar şu sırayla hazırlanmıştır:
1. down/front
2. left
3. right
4. up/back

Robot kare adları:
- idle
- hover1
- turn
- hover2
- turn_alt

## Önerilen Unity Import Ayarları
- Texture Type: Sprite (2D and UI)
- Sprite Mode: Single (tekil PNG’ler için) / Multiple (sheet kullanacaksan)
- Filter Mode: Point (no filter)
- Compression: None
- Mesh Type: Full Rect

## Önerilen Animasyon FPS
- Player Walk: 8 FPS
- Robot Hover: 7 FPS
- Electric Barrier: 10 FPS
- Sliding Door: 8 FPS
- Evidence Terminal: 6 FPS
- ARK-13 Core: 6 FPS

## Bölüm Akışı
Start room -> corridor -> electric hazard room -> side route switch room (robot) -> evidence terminal room -> final ARK-13 room.

## Not
Bu paket görsel olarak kullanılabilir olacak şekilde düzenlenmiştir. Unity tarafında sprite slicing, animator controller, collision ve gameplay scriptleri ayrıca kurulmalıdır.


## Yeni Eklenen Map Ana Bileşenleri
- Assets/Karantina13/MapCore klasörü eklendi.
- Bu klasörde duvar, yer ve kapı gibi temel harita birleşenleri tek tek ayrı PNG dosyaları halinde bulunur.
- Ek dosyalar:
  - MapCore/MapCore_Preview.png
  - MapCore/MapCore_Tileset.png
