namespace Portal.Models
{
    public enum EntryType
    {
        Income = 1,
        Expense = 2
    }

    public enum RecurrenceType
    {
        Once = 1,
        Monthly = 2,
        Yearly = 3
    }

    public enum QuestionType
    {
        Simple = 1,
        MultipleChoice = 2,
        TrueFalse = 3,
        FillInTheBlank = 4
    }
}