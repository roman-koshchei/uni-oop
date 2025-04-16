namespace Server;

public class Student(string fullName, int grade)
{
    public string Name { get; set; } = fullName;

    public int Grade { get; set; } = grade;
}