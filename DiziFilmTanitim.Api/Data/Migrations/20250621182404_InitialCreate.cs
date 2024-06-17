using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DiziFilmTanitim.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kullanicilar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Sifre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kullanicilar", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Oyuncular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Biyografi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FotografDosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Oyuncular", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Turler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Yonetmenler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdSoyad = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DogumTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Biyografi = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FotografDosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Yonetmenler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciListeleri",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListeAdi = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KullaniciId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciListeleri", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KullaniciListeleri_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Diziler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    YapimYili = table.Column<int>(type: "int", nullable: true),
                    Ozet = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AfisDosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Durum = table.Column<int>(type: "int", nullable: false),
                    YonetmenId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diziler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Diziler_Yonetmenler_YonetmenId",
                        column: x => x.YonetmenId,
                        principalTable: "Yonetmenler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Filmler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    YapimYili = table.Column<int>(type: "int", nullable: true),
                    Ozet = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AfisDosyaAdi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SureDakika = table.Column<int>(type: "int", nullable: true),
                    YonetmenId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filmler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filmler_Yonetmenler_YonetmenId",
                        column: x => x.YonetmenId,
                        principalTable: "Yonetmenler",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DiziKullaniciListesi",
                columns: table => new
                {
                    DizilerId = table.Column<int>(type: "int", nullable: false),
                    KullaniciListeleriId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiziKullaniciListesi", x => new { x.DizilerId, x.KullaniciListeleriId });
                    table.ForeignKey(
                        name: "FK_DiziKullaniciListesi_Diziler_DizilerId",
                        column: x => x.DizilerId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiziKullaniciListesi_KullaniciListeleri_KullaniciListeleriId",
                        column: x => x.KullaniciListeleriId,
                        principalTable: "KullaniciListeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiziOyuncu",
                columns: table => new
                {
                    DizilerId = table.Column<int>(type: "int", nullable: false),
                    OyuncularId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiziOyuncu", x => new { x.DizilerId, x.OyuncularId });
                    table.ForeignKey(
                        name: "FK_DiziOyuncu_Diziler_DizilerId",
                        column: x => x.DizilerId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiziOyuncu_Oyuncular_OyuncularId",
                        column: x => x.OyuncularId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiziTur",
                columns: table => new
                {
                    DizilerId = table.Column<int>(type: "int", nullable: false),
                    TurlerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiziTur", x => new { x.DizilerId, x.TurlerId });
                    table.ForeignKey(
                        name: "FK_DiziTur_Diziler_DizilerId",
                        column: x => x.DizilerId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiziTur_Turler_TurlerId",
                        column: x => x.TurlerId,
                        principalTable: "Turler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciDiziPuanlari",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    DiziId = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciDiziPuanlari", x => new { x.KullaniciId, x.DiziId });
                    table.ForeignKey(
                        name: "FK_KullaniciDiziPuanlari_Diziler_DiziId",
                        column: x => x.DiziId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciDiziPuanlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sezonlar",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SezonNumarasi = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiziId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sezonlar", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sezonlar_Diziler_DiziId",
                        column: x => x.DiziId,
                        principalTable: "Diziler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmKullaniciListesi",
                columns: table => new
                {
                    FilmlerId = table.Column<int>(type: "int", nullable: false),
                    KullaniciListeleriId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmKullaniciListesi", x => new { x.FilmlerId, x.KullaniciListeleriId });
                    table.ForeignKey(
                        name: "FK_FilmKullaniciListesi_Filmler_FilmlerId",
                        column: x => x.FilmlerId,
                        principalTable: "Filmler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmKullaniciListesi_KullaniciListeleri_KullaniciListeleriId",
                        column: x => x.KullaniciListeleriId,
                        principalTable: "KullaniciListeleri",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmOyuncu",
                columns: table => new
                {
                    FilmlerId = table.Column<int>(type: "int", nullable: false),
                    OyuncularId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmOyuncu", x => new { x.FilmlerId, x.OyuncularId });
                    table.ForeignKey(
                        name: "FK_FilmOyuncu_Filmler_FilmlerId",
                        column: x => x.FilmlerId,
                        principalTable: "Filmler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmOyuncu_Oyuncular_OyuncularId",
                        column: x => x.OyuncularId,
                        principalTable: "Oyuncular",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FilmTur",
                columns: table => new
                {
                    FilmlerId = table.Column<int>(type: "int", nullable: false),
                    TurlerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilmTur", x => new { x.FilmlerId, x.TurlerId });
                    table.ForeignKey(
                        name: "FK_FilmTur_Filmler_FilmlerId",
                        column: x => x.FilmlerId,
                        principalTable: "Filmler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FilmTur_Turler_TurlerId",
                        column: x => x.TurlerId,
                        principalTable: "Turler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KullaniciFilmPuanlari",
                columns: table => new
                {
                    KullaniciId = table.Column<int>(type: "int", nullable: false),
                    FilmId = table.Column<int>(type: "int", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciFilmPuanlari", x => new { x.KullaniciId, x.FilmId });
                    table.ForeignKey(
                        name: "FK_KullaniciFilmPuanlari_Filmler_FilmId",
                        column: x => x.FilmId,
                        principalTable: "Filmler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KullaniciFilmPuanlari_Kullanicilar_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "Kullanicilar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bolumler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BolumNumarasi = table.Column<int>(type: "int", nullable: false),
                    Ad = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Ozet = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    YayinTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SureDakika = table.Column<int>(type: "int", nullable: true),
                    SezonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bolumler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bolumler_Sezonlar_SezonId",
                        column: x => x.SezonId,
                        principalTable: "Sezonlar",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bolumler_SezonId",
                table: "Bolumler",
                column: "SezonId");

            migrationBuilder.CreateIndex(
                name: "IX_DiziKullaniciListesi_KullaniciListeleriId",
                table: "DiziKullaniciListesi",
                column: "KullaniciListeleriId");

            migrationBuilder.CreateIndex(
                name: "IX_Diziler_YonetmenId",
                table: "Diziler",
                column: "YonetmenId");

            migrationBuilder.CreateIndex(
                name: "IX_DiziOyuncu_OyuncularId",
                table: "DiziOyuncu",
                column: "OyuncularId");

            migrationBuilder.CreateIndex(
                name: "IX_DiziTur_TurlerId",
                table: "DiziTur",
                column: "TurlerId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmKullaniciListesi_KullaniciListeleriId",
                table: "FilmKullaniciListesi",
                column: "KullaniciListeleriId");

            migrationBuilder.CreateIndex(
                name: "IX_Filmler_YonetmenId",
                table: "Filmler",
                column: "YonetmenId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmOyuncu_OyuncularId",
                table: "FilmOyuncu",
                column: "OyuncularId");

            migrationBuilder.CreateIndex(
                name: "IX_FilmTur_TurlerId",
                table: "FilmTur",
                column: "TurlerId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciDiziPuanlari_DiziId",
                table: "KullaniciDiziPuanlari",
                column: "DiziId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciFilmPuanlari_FilmId",
                table: "KullaniciFilmPuanlari",
                column: "FilmId");

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciListeleri_KullaniciId",
                table: "KullaniciListeleri",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_Sezonlar_DiziId",
                table: "Sezonlar",
                column: "DiziId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bolumler");

            migrationBuilder.DropTable(
                name: "DiziKullaniciListesi");

            migrationBuilder.DropTable(
                name: "DiziOyuncu");

            migrationBuilder.DropTable(
                name: "DiziTur");

            migrationBuilder.DropTable(
                name: "FilmKullaniciListesi");

            migrationBuilder.DropTable(
                name: "FilmOyuncu");

            migrationBuilder.DropTable(
                name: "FilmTur");

            migrationBuilder.DropTable(
                name: "KullaniciDiziPuanlari");

            migrationBuilder.DropTable(
                name: "KullaniciFilmPuanlari");

            migrationBuilder.DropTable(
                name: "Sezonlar");

            migrationBuilder.DropTable(
                name: "KullaniciListeleri");

            migrationBuilder.DropTable(
                name: "Oyuncular");

            migrationBuilder.DropTable(
                name: "Turler");

            migrationBuilder.DropTable(
                name: "Filmler");

            migrationBuilder.DropTable(
                name: "Diziler");

            migrationBuilder.DropTable(
                name: "Kullanicilar");

            migrationBuilder.DropTable(
                name: "Yonetmenler");
        }
    }
}
