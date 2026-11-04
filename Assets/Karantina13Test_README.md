# Karantina 13 test alanı

`Assets/Scenes/Karantina13_TestArea.unity` sahnesini aç ve Play'e bas.
Game penceresine tıklayarak klavye odağını ver.

- **WASD:** Player hareketi ve dört yönlü yürüme animasyonu (8 FPS).
- **Ok tuşları:** Robot hareketi ve dört yönlü hover animasyonu (7 FPS).
- **R:** İki karakteri başlangıç konumuna getir.

İki karakter aynı anda kontrol edilebilir. Tuş bırakıldığında karakter son
baktığı yönde bekler. Çapraz harekette hız artmaz. Duvarlar, kutular ve
laboratuvar eşyaları fiziksel engeldir. Kamera bütün alanı gösterir.

Karakter prefabları ve Animator controller/clip dosyaları `Assets/Karantina13Test`
klasöründedir. Hızlar karakterlerin `KarantinaCharacterController` bileşeninden
değiştirilebilir. Animasyonlar Animator penceresinde incelenebilir; kontrol
bileşeni hareket durumuna ve yöne göre ilgili state'i seçer.

Sahneyi tekrar kurmak için Unity menüsünden **Karantina 13 > Test Alanini Olustur**.
Bu işlem test sahnesini yeniden oluşturur. Sahne Build Settings listesinde ilk
sıradadır; mevcut sahneler korunmuştur.
