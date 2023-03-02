using Microsoft.EntityFrameworkCore.Migrations;

namespace HolyQuran.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Readers",
                columns: table => new
                {
                    Read = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    NameView = table.Column<string>(nullable: true),
                    Location = table.Column<string>(nullable: true),
                    BirthDate = table.Column<string>(nullable: true),
                    DeathDate = table.Column<string>(nullable: true),
                    Info = table.Column<string>(nullable: true),
                    Rowat = table.Column<int>(nullable: false),
                    RawyInfo = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readers", x => x.Read);
                });

            migrationBuilder.CreateTable(
                name: "Suras",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Order = table.Column<int>(nullable: false),
                    HolySurahNameAr = table.Column<string>(nullable: true),
                    HolySurahNameEn = table.Column<string>(nullable: true),
                    Landing = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ayat",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SorahId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ayat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ayat_Suras_SorahId",
                        column: x => x.SorahId,
                        principalTable: "Suras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RawyText",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SurahId = table.Column<int>(nullable: false),
                    AyahId = table.Column<int>(nullable: false),
                    AyaNumber = table.Column<int>(nullable: false),
                    Text = table.Column<string>(nullable: true),
                    Rawy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawyText", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RawyText_Ayat_AyahId",
                        column: x => x.AyahId,
                        principalTable: "Ayat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reading",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AyahId = table.Column<int>(nullable: false),
                    AyaNumber = table.Column<int>(nullable: false),
                    Reader = table.Column<int>(nullable: false),
                    ReadView = table.Column<string>(nullable: true),
                    HolyRead = table.Column<string>(nullable: true),
                    ReadInfo = table.Column<string>(nullable: true),
                    AgreedOn = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reading_Ayat_AyahId",
                        column: x => x.AyahId,
                        principalTable: "Ayat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ayat_SorahId",
                table: "Ayat",
                column: "SorahId");

            migrationBuilder.CreateIndex(
                name: "IX_RawyText_AyahId",
                table: "RawyText",
                column: "AyahId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reading_AyahId",
                table: "Reading",
                column: "AyahId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RawyText");

            migrationBuilder.DropTable(
                name: "Readers");

            migrationBuilder.DropTable(
                name: "Reading");

            migrationBuilder.DropTable(
                name: "Ayat");

            migrationBuilder.DropTable(
                name: "Suras");
        }
    }
}
