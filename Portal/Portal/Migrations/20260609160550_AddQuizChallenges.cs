using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Portal.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "challenges",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    challenge_id = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_questions", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_questions_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    challenge_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    score = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_submissions", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_submissions_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quiz_submissions_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_correct = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_options_quiz_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    submission_id = table.Column<int>(type: "int", nullable: false),
                    question_id = table.Column<int>(type: "int", nullable: false),
                    selected_option_id = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_answers", x => x.id);
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_options_selected_option_id",
                        column: x => x.selected_option_id,
                        principalTable: "quiz_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "quiz_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_submissions_submission_id",
                        column: x => x.submission_id,
                        principalTable: "quiz_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_question_id",
                table: "quiz_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_selected_option_id",
                table: "quiz_answers",
                column: "selected_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_submission_id",
                table: "quiz_answers",
                column: "submission_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_options_question_id",
                table: "quiz_options",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_questions_challenge_id",
                table: "quiz_questions",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_submissions_challenge_id",
                table: "quiz_submissions",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_submissions_student_id",
                table: "quiz_submissions",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_options");

            migrationBuilder.DropTable(
                name: "quiz_submissions");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropColumn(
                name: "type",
                table: "challenges");
        }
    }
}
