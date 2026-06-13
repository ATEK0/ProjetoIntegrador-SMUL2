using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Migrations
{
    /// <inheritdoc />
    public partial class UpgradeChallengeSubmissionsToDynamic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "multiple_choice_answer",
                table: "challenge_submissions");

            migrationBuilder.DropColumn(
                name: "simple_question_answer",
                table: "challenge_submissions");

            migrationBuilder.DropColumn(
                name: "true_false_answer",
                table: "challenge_submissions");

            migrationBuilder.AddColumn<string>(
                name: "feedback",
                table: "challenge_submissions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "graded_at",
                table: "challenge_submissions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "challenge_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    challenge_id = table.Column<int>(type: "int", nullable: false),
                    question_text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    question_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_a = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_b = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_c = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    option_d = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    correct_answer = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_challenge_questions_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "challenge_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    challenge_submission_id = table.Column<int>(type: "int", nullable: false),
                    challenge_question_id = table.Column<int>(type: "int", nullable: false),
                    answer_text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_correct = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_challenge_answers_challenge_questions_challenge_question_id",
                        column: x => x.challenge_question_id,
                        principalTable: "challenge_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_challenge_answers_challenge_submissions_challenge_submission",
                        column: x => x.challenge_submission_id,
                        principalTable: "challenge_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_answers_challenge_question_id",
                table: "challenge_answers",
                column: "challenge_question_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_answers_challenge_submission_id_challenge_question",
                table: "challenge_answers",
                columns: new[] { "challenge_submission_id", "challenge_question_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "ix_challenge_questions_challenge_id_deleted_at",
                table: "challenge_questions",
                columns: new[] { "challenge_id", "deleted_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "challenge_answers");

            migrationBuilder.DropTable(
                name: "challenge_questions");

            migrationBuilder.DropColumn(
                name: "feedback",
                table: "challenge_submissions");

            migrationBuilder.DropColumn(
                name: "graded_at",
                table: "challenge_submissions");

            migrationBuilder.AddColumn<string>(
                name: "multiple_choice_answer",
                table: "challenge_submissions",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "simple_question_answer",
                table: "challenge_submissions",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "true_false_answer",
                table: "challenge_submissions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
