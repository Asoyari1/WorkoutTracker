using workoutTracker.models;
using Microsoft.EntityFrameworkCore;
using workoutTracker.Data;
using System;
using System.IO;

public class Program
{
    public static void Main(string[] args)
    {

        string dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "workoutTracker.db"));

        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite($"Data Source={dbPath}")
        .Options;

        using var db = new AppDbContext(options);

        Console.WriteLine($"Database path: {dbPath}");
        Console.WriteLine("--------------");
        Console.WriteLine("WorkoutTracker");
        Console.WriteLine("--------------");

        var exercises = new List<Exercise>();
        var workouts = new List<Workout>();
        string choice;
        while (true)
        {
            Console.WriteLine("1. Add exercise");
            Console.WriteLine("2. View exercises");
            Console.WriteLine("3. Create workout");
            Console.WriteLine("4. View workouts");
            Console.WriteLine("5. Delete exercise");
            Console.WriteLine("6. Update exercise");
            Console.WriteLine("7. Search exercises");
            Console.WriteLine("8. Exit");
            Console.Write("choose: ");
            choice = Console.ReadLine() ?? "";

            switch (choice)
            {
                case "1":
                    {
                        AddExercise(db);
                        break;
                    }
                case "2":
                    {
                        ViewExercises(db);
                        break;
                    }
                case "3":
                    {
                        CreateWorkout(db);
                        break;
                    }
                case "4":
                    {
                        ViewWorkouts(db);
                        break;
                    }
                case "5":
                    {
                        DeleteExercise(db);
                        break;
                    }
                case "6":
                    {
                        UpdateExercise(db);
                        break;
                    }
                case "7":
                    {
                        SearchExercises(db);
                        break;
                    }
                case "8":
                    {
                        ViewFrequentlyUsedExercises(db);
                        break;
                    }
                case "9":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1, 2, 3, 4, 5, 6, 7 or 8.");
                    break;
            }
        }
    }

    static void AddExercise(AppDbContext db)
    {
        Console.Write("Exercise name: ");
        string exerciseName = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(exerciseName))
        {
            Console.WriteLine("Exercise name cannot be empty.");
            return;
        }

        Console.Write("Muscle group: ");
        string muscleGroup = Console.ReadLine() ?? "";

        if (string.IsNullOrWhiteSpace(muscleGroup))
        {
            Console.WriteLine("Muscle group cannot be empty.");
            return;
        }
        var exercises = db.Exercises.ToList();
        if(exercises.Any(e => e.Name.Equals(exerciseName, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Exercise '{exerciseName}' already exists.");
            return;
        }

        var exercise = new Exercise(exerciseName, muscleGroup);

        db.Exercises.Add(exercise);
        db.SaveChanges();

        Console.WriteLine($"Exercise added: {exerciseName} ({muscleGroup})");
    }

    static void ViewExercises(AppDbContext db)
    {
        var exercises = db.Exercises.ToList();
        Console.WriteLine("View exercises added.");
        if (exercises.Count == 0)
        {
            Console.WriteLine("  (no exercises)");
            return;
        }
        int num = 1;

        foreach (var exercise in exercises)
        {
            Console.WriteLine(
                $"{num}. {exercise.Name} ({exercise.MuscleGroup})");
            num++;
        }
        Console.WriteLine();
    }

    static void CreateWorkout(AppDbContext db)
    {
        while (true)
        {
            Console.Write("Workout name: ");
            string workoutName = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(workoutName))
            {
                Console.WriteLine("Workout name cannot be empty. Please enter a valid name.");
                continue;
            }
            if (db.Exercises.Count() == 0)
            {
                Console.WriteLine("No exercises available. Add exercises first.");
                return;
            }
            var workout = new Workout(workoutName);
            db.Add(workout);

            Console.WriteLine($"Workout created: {workoutName}");

            Console.WriteLine("===== AVAILABLE EXERCISES =====");

            while (true)
            {
                var addedExerciseNames = workout.Exercises
                    .Select(wex => wex.Name)
                    .ToList();

                var remaining = db.Exercises
                    .ToList()
                    .Where(ex => !addedExerciseNames
                        .Any(name => name.Equals(
                            ex.Name,
                            StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (remaining.Count == 0)
                {
                    db.SaveChanges();
                    Console.WriteLine("All exercises have been added to the workout.");
                    return;
                }

                int num = 1;
                foreach (var ex in remaining)
                {
                    Console.WriteLine($"{num}. {ex.Name} ({ex.MuscleGroup})");
                    num++;
                }

                Console.Write("Choose an exercise number to add: ");
                var input = Console.ReadLine() ?? "";

                if (int.TryParse(input, out int index) &&
                    index >= 1 &&
                    index <= remaining.Count)
                {
                    var selected = remaining[index - 1];
                    workout.AddExercise(selected);
                    Console.WriteLine($"{selected.Name} added to {workoutName}");
                }
                else
                {
                    Console.WriteLine("Invalid exercise number.");
                    continue;
                }

                string another;
                while (true)
                {
                    Console.Write("Add another exercise? (y/n): ");
                    another = (Console.ReadLine() ?? "").Trim().ToLower();
                    if (another == "y" || another == "n") break;
                    Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                }

                if (another == "n")
                {
                    break;
                }

                Console.WriteLine();
            }
            db.SaveChanges();

            break;
        }
    }

    static void ViewWorkouts(AppDbContext db)
    {
        Console.WriteLine("===== WORKOUTS =====");
        var workouts =db.Workouts.Include(w => w.Exercises).ToList();
        foreach (var workout in db.Workouts)
        {
            Console.WriteLine($"Workout: {workout.Name}");
            Console.WriteLine("Exercises:");
            foreach (var ex in workout.Exercises)
            {
                Console.WriteLine($"- {ex.Name} ({ex.MuscleGroup})");
            }
        }
    }
    static void DeleteExercise(AppDbContext db)
    {
        while (true)
        {
            var exercises = db.Exercises.ToList();

            if (exercises.Count == 0)
            {
                Console.WriteLine("No exercises available to delete.");
                return;
            }

            Console.WriteLine("Which exercise would you like to delete? (enter number, or 'c' to cancel)");
            for (int i = 0; i < exercises.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {exercises[i].Name} ({exercises[i].MuscleGroup})");
            }

            var input = (Console.ReadLine() ?? "").Trim();

            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            if (!int.TryParse(input, out int number))
            {
                Console.WriteLine("Please enter a valid number.");
                continue; // keep asking
            }

            if (number < 1 || number > exercises.Count)
            {
                Console.WriteLine($"Invalid exercise number. Enter a number between 1 and {exercises.Count}.");
                continue; // keep asking
            }

            var exerciseToRemove = exercises[number - 1];

            string confirm;
            Console.Write("Enter exercise ID: ");

            if (!int.TryParse(Console.ReadLine(), out int selectedId))
            {
                Console.WriteLine("Invalid ID.");
                return;
            }
            var exerciseById = db.Exercises.FirstOrDefault(e => e.Id == selectedId);
            if(exerciseById == null)
            {
                Console.WriteLine($"No exercise found with ID {selectedId}.");
                return;
            }
            Console.WriteLine($"Exercise found: {exerciseById.Name} ({exerciseById.MuscleGroup})");
            Console.WriteLine("are you sure you want to delete this exercise y/n?");
            var confirmation = (Console.ReadLine()?? "").Trim().ToLower();
            if(confirmation.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                db.Exercises.Remove(exerciseById);
                db.SaveChanges();
                Console.WriteLine("Exercise deleted successfully.");
            }
            else if (confirmation.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Deletion cancelled.");
            }
            else
            {
                Console.WriteLine("Invalid choice. Deletion cancelled.");
            }



            while (true)
            {
                Console.Write("Are you sure? (y/n): ");
                confirm = (Console.ReadLine() ?? "").Trim().ToLower();

                if (confirm == "y" || confirm == "n")
                    break;

                Console.WriteLine("Please enter 'y' or 'n'.");
            }

            if (confirm == "y")
            {
                db.Exercises.Remove(exerciseToRemove);
                db.SaveChanges();
                Console.WriteLine("Exercise deleted.");
            }
            else
            {
                Console.WriteLine("Deletion cancelled.");
            }
            return; // done -> back to menu
        }

    }
    static void UpdateExercise(AppDbContext db)
    {
        while (true)
        {
            var exercises = db.Exercises.ToList();
            if (exercises.Count == 0)
            {
                Console.WriteLine("No exercises available to update.");
                return;
            }
            Console.WriteLine("Which exercise would you like to update? (enter number, or 'c' to cancel)");
            for (int i = 0; i < exercises.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {exercises[i].Name} ({exercises[i].MuscleGroup})");
            }
            var input = (Console.ReadLine() ?? "").Trim();
            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return;
            }
            if (!int.TryParse(input, out int number))
            {
                Console.WriteLine("Please enter a valid number.");
                continue; // keep asking
            }
            if (number < 1 || number > exercises.Count)
            {
                Console.WriteLine($"Invalid exercise number. Enter a number between 1 and {exercises.Count}.");
                continue; // keep asking
            }
            bool changed = false;
            var exerciseToUpdate = exercises[number - 1];
            Console.Write($"Enter new name for '{exerciseToUpdate.Name}' (or press Enter to keep current): ");
            string newName = (Console.ReadLine() ?? "").Trim();
            Console.Write($"Enter new muscle group for '{exerciseToUpdate.MuscleGroup}' (or press Enter to keep current): ");
            string newMuscleGroup = (Console.ReadLine() ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(newName))
            {
                exerciseToUpdate.Name = newName;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(newMuscleGroup))
            {
                exerciseToUpdate.MuscleGroup = newMuscleGroup;
                changed = true;
            }
            if (changed)
            {
                db.SaveChanges();
                Console.WriteLine("Exercise updated.");
            }
            else
            {
                Console.WriteLine("No changes made.");
            }
            return; // done -> back to menu
        }
    }
    static void ViewChestExercises(AppDbContext db)
    {
        var exerciseNames = db.Exercises.Where(ex => ex.MuscleGroup.ToLower() == "chest")
            .Select(ex => ex.Name)
            .ToList();
        foreach (var name in exerciseNames)
        {
            Console.WriteLine(name);
        }
    }
    static void ViewExerciseSummary(AppDbContext db)
    {
       var exercises = db.Exercises.Select(ex => new { ex.Name, ex.MuscleGroup }).OrderBy(ex => ex.MuscleGroup).ThenBy(ex => ex.Name).ToList();
        Console.WriteLine("Exercise Summary:");
        foreach (var exercise in exercises)
        {
            Console.WriteLine($"- {exercise.Name} ({exercise.MuscleGroup})");
        }
    }
    static void ViewExercisesByMuscleGroup(AppDbContext db)
    {
        var exercisesByMuscleGroup = db.Exercises.GroupBy(ex => ex.MuscleGroup).OrderBy(g => g.Key)
            .Select(g => new { MuscleGroup = g.Key, Exercises = g.Select(ex => ex.Name).OrderBy(name => name).ToList() })
            .ToList();
        foreach(var group in exercisesByMuscleGroup)
        {
            Console.WriteLine($"Muscle Group: {group.MuscleGroup}");
            foreach(var exercise in group.Exercises)
            {
                Console.WriteLine($"- {exercise}");
            }
            Console.WriteLine();
        }
    }
    static void ViewAllWorkoutExerciseNames(AppDbContext db)
    {
        var exerciseNames = db.Workouts.Include(w => w.Exercises)
            .SelectMany(w => w.Exercises.Select(ex => ex.Name))
            .Distinct()
            .OrderBy(name => name).ToList();
        Console.WriteLine("All Workout Exercise Names:");
        foreach (var name in exerciseNames)
        {
            Console.WriteLine($"- {name}");
        }
    }
    static void ViewUnusedExercises(AppDbContext db)
    {
        var unusedExercises = db.Exercises
            .Where(ex => !ex.Workouts.Any())
            .OrderBy(ex => ex.Name)
            .ToList();
        foreach (var exercise in unusedExercises)
        {
            Console.WriteLine($"- {exercise.Name} ({exercise.MuscleGroup})");
        }
    }
    static void ViewExerciseUsage(AppDbContext db)
    {
        var exerciseUsage = db.Exercises
            .Select(ex => new { ex.Name, UsageCount = ex.Workouts.Count() })
            .OrderByDescending(ex => ex.UsageCount)
            .ThenBy(ex => ex.Name).ToList();
        foreach (var exercise in exerciseUsage)
        {
            Console.WriteLine($"- {exercise.Name}: {exercise.UsageCount} workout(s)");
        }
    }
    static void ViewFrequentlyUsedExercises(AppDbContext db)
    {
        Console.WriteLine("Show exercises used in at least how many workouts?");
        if (!int.TryParse(Console.ReadLine(), out int thresholdValue)
        || thresholdValue < 0)
        {
            Console.WriteLine("Please enter zero or a positive integer.");
            return;
        }
        var frequentlyUsedExercises = db.Exercises
            .Select(ex => new {ex.Name,ex.MuscleGroup, UsageCount = ex.Workouts.Count()})
            .Where(ex => ex.UsageCount >= thresholdValue)
            .OrderByDescending(ex => ex.UsageCount).ThenBy(ex => ex.Name)
            .ToList();
        foreach (var exercise in frequentlyUsedExercises)
        {
            Console.WriteLine($"- {exercise.Name} ({exercise.MuscleGroup}): {exercise.UsageCount} workout(s)");
        }
        string input = "hello";

        try
        {
            string text = File.ReadAllText("number.txt");
            int number = int.Parse(text);

            Console.WriteLine(number);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("The file 'number.txt' was not found.");
        }
        catch (FormatException)
        {
            Console.WriteLine("The content of 'number.txt' is not a valid integer.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Finished reading the file.");
        }

    }
    static void SearchExercises(AppDbContext db)
    {
        Console.Write("what do you want to search for?: ");
        string searchTerm = (Console.ReadLine() ?? "").Trim().ToLower();
        if(string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        var exercises = db.Exercises.ToList();
        var filteredExercises = exercises.Where(ex => ex.MuscleGroup.ToLower().Contains(searchTerm) || ex.Name.ToLower().Contains(searchTerm)).ToList();

        if (filteredExercises.Count == 0)
        {
            Console.WriteLine("No exercises found.");
            return;
        }

        Console.WriteLine("Search results:");
        int num = 1;
        foreach (var exercise in filteredExercises)
        {
            Console.WriteLine($"{num}. {exercise.Name} ({exercise.MuscleGroup})");
            num++;
        }
        
    }

}