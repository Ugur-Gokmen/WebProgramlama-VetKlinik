# Pati Ev — Veteriner Kliniği Randevu ve Hasta Takip Sistemi

> **Web Programlama Dersi Proje Ödevi (2026-2027 Güz)**

| Bilgi | Değer |
|---|---|
| **Öğrenci Numarası** | `B241210053` |
| **Adı Soyadı** | `Uğur Gökmen` |
| **Ders Grubu** | `1.Öğretim B` |
| **GitHub Bağlantısı** | `https://github.com/Ugur-Gokmen/WebProgramlama-VetKlinik` |

---

## Proje Tanıtımı

**Pati Ev**, ASP.NET Core MVC (.NET 10) kullanılarak geliştirilen bir **Veteriner Kliniği Randevu ve Hasta Takip Sistemi**dir. Uygulama; kliniklerin ve veteriner hekimlerin yönetimini, evcil hayvanların aşı ve muayene geçmişinin takibini, hizmet süresi ve hekim müsaitliği tabanlı çakışma kontrollü randevu oluşturmayı sağlar.

Sistem üç kanal üzerinden çalışır ve tüm kanallar **`Services/`** altındaki aynı iş mantığı katmanını kullanır:
1. **MVC Web Arayüzü** (Bootstrap 5 + jQuery Validation)
2. **REST API** (DTO tabanlı, LINQ filtrelemeli, Swagger/OpenAPI destekli)
3. **MCP Sunucusu** (`/mcp`, `ModelContextProtocol.AspNetCore` ile yapay zekâ asistanı entegrasyonu)

---

## Kullanılan Teknolojiler

- **Backend:** ASP.NET Core MVC (.NET 10 LTS), C#
- **Veritabanı ve ORM:** SQLite (SQL Server'a geçilebilir yapıda), Entity Framework Core (Code First + Migration), LINQ
- **Kimlik Doğrulama & Yetkilendirme:** ASP.NET Core Identity (`Admin`, `Veteriner`, `Uye`) + Kullanıcı bazlı SHA-256 hash'li API Anahtarı (`Authorization: Bearer`)
- **Yapay Zekâ Entegrasyonu:** `ModelContextProtocol.AspNetCore` (HTTP transport)
- **API Dokümantasyonu:** `Swashbuckle.AspNetCore` (Swagger UI)
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript, jQuery & jQuery Unobtrusive Validation
- **Test:** xUnit (`VetKlinik.Tests`)

---

## Klasör Yapısı

```text
WebProje/
├── VetKlinik.sln
├── README.md
├── .gitignore
├── VetKlinik/
│   ├── Api/             # REST API controller'ları
│   ├── Areas/Admin/     # Admin yönetim paneli (Klinik, Hizmet, Veteriner, Randevu)
│   ├── Auth/            # ApiKeyAuthenticationHandler ve ClaimsPrincipal yardımcıları
│   ├── Controllers/     # MVC controller'ları (Hesap, Hayvanlar, Asilar, Randevular, VeterinerPanel, Profil)
│   ├── Data/            # UygulamaDbContext ve VeriTohumu (seed)
│   ├── Dtos/            # REST API DTO sınıfları
│   ├── Mcp/             # MCP sunucusu araçları
│   ├── Models/          # Veritabanı entity sınıfları
│   ├── Services/        # Ortak iş kuralları (RandevuServisi, HayvanServisi vb.)
│   ├── ViewModels/      # Form ve görünüm modelleri
│   └── Views/           # Razor görünümleri
└── VetKlinik.Tests/     # xUnit birim ve güvenlik testleri
```

---

## Kurulum ve Çalıştırma

### Ön Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/) (`dotnet --version` ile kontrol edebilirsiniz)

### Gizli Ayarlar (`user-secrets`)
Veritabanı parolası veya harici anahtar gibi gizli bilgiler depoya **eklenmez**. Geliştirme ortamında gizli ayar tanımlamak için:

```bash
dotnet user-secrets set "AnahtarAdi" "Deger" --project VetKlinik
```

### Çalıştırma
```bash
dotnet build
dotnet test
dotnet run --project VetKlinik
```

Uygulama açılışta otomatik olarak migration ve başlangıç verilerini (roller, admin, örnek klinik, hizmetler, hekimler, örnek üye/hayvan/aşı) oluşturur.

---

## Veritabanı Modeli (ER Diyagramı)

> *(M9 aşamasında Mermaid ER diyagramı eklenecektir.)*

---

## Ekran Görüntüleri

> *(M9 aşamasında aşağıdaki ekran görüntüleri eklenecektir.)*
- [ ] Kayıt ve Giriş sayfaları
- [ ] Admin Paneli CRUD ekranları (Klinik, Hizmet, Veteriner)
- [ ] Randevu alma ve çakışma uyarısı
- [ ] Veteriner paneli (Randevu onay/red ve muayene notu)
- [ ] Swagger UI (`/swagger`)
- [ ] MCP Inspector üzerinden araç çağrısı (`/mcp`)

---

## REST API Uçları

> *(M6 / M9 aşamasında örnek istek ve cevaplarla detaylandırılacaktır.)*

| Metot | Uç Nokta | Açıklama | Yetki |
|---|---|---|---|
| `GET` | `/api/veterinerler` | Tüm veteriner hekimleri listeler | Giriş yapmış |
| `GET` | `/api/veterinerler/uygun?hizmetId=&tarih=` | Belirli tarihte hizmet için uygun hekimleri getirir | Giriş yapmış |
| `GET` | `/api/hayvanlar/{id}/asi-gecmisi` | Evcil hayvanın geçmiş ve yaklaşan aşılarını getirir | Yalnızca sahibi |
| `GET` | `/api/raporlar/hizmet-ozeti` | Hizmet türüne göre randevu sayısı ve ciro raporu | Admin |

---

## MCP Sunucusu ve Araçları

> *(M7 / M9 aşamasında MCP Inspector test adımlarıyla detaylandırılacaktır.)*

- `hizmetleri_listele`
- `evcil_hayvanlarim`
- `uygun_saatler(hizmetId, tarih)`
- `asi_takvimi(hayvanAdi)`
- `randevu_olustur(hayvanAdi, hizmetId, hekimId, baslangic)`

---

## Teslim Kontrol Listesi

- [ ] Repo herkese açık, ilk commit'te `README.md` mevcut
- [ ] En az 10 farklı günde 10 farklı commit
- [ ] Depoda parola / API anahtarı yok
- [ ] `README.md` başında öğrenci bilgileri ve GitHub bağlantısı eksiksiz
- [ ] ER diyagramı, ekran görüntüleri, REST API ve MCP örnekleri eklendi
- [ ] Üç rol (`Admin`, `Veteriner`, `Uye`) ve IDOR koruması doğrulandı
- [ ] SABİS'e 3 satırlık `.txt` dosyası yüklendi
