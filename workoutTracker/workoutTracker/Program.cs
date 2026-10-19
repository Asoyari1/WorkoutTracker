using Microsoft.EntityFrameworkCore;
using workoutTracker.Data;
using workoutTracker.models;
using workoutTracker.Repositories;
using Microsoft.Extensions.DependencyInjection;



public class Program
{
    public static void Main(string[] args)
    {
  
        string dbPath = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "workoutTracker.db"));

        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlite($"Data Source={dbPath}")
        .Options;

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var exerciseRepository =
            scope.ServiceProvider.GetRequiredService<IExerciseRepository>();

        var workoutRepository =
            scope.ServiceProvider.GetRequiredService<IWorkoutRepository>();

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
                        AddExercise(exerciseRepository);
                        break;
                    }
                case "2":
                    {
                        ViewExercises(exerciseRepository);
                        break;
                    }
                case "3":
                    {
                        CreateWorkout(exerciseRepository, workoutRepository);
                        break;
                    }
                case "4":
                    {
                        ViewWorkouts(workoutRepository);
                        break;
                    }
                case "5":
                    {
                        DeleteExercise(exerciseRepository);
                        break;
                    }
                case "6":
                    {
                        UpdateExercise(exerciseRepository);
                        break;
                    }
                case "7":
                    {
                        SearchExercises(exerciseRepository);
                        break;
                    }
                case "8":
                    {
                        ViewFrequentlyUsedExercises(exerciseRepository);
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

    static void AddExercise(IExerciseRepository exerciseRepository)
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
        var exercises = exerciseRepository.GetAll();
        if(exercises.Any(e => e.Name.Equals(exerciseName, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine($"Exercise '{exerciseName}' already exists.");
            return;
        }

        var exercise = new Exercise(exerciseName, muscleGroup);

        exerciseRepository.Add(exercise);

        Console.WriteLine($"Exercise added: {exerciseName} ({muscleGroup})");
    }

    static void ViewExercises(IExerciseRepository exerciseRepository)
    {
        var exercises = exerciseRepository.GetAll();
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

    static void CreateWorkout(
    IExerciseRepository exerciseRepository,
    IWorkoutRepository workoutRepository)
    {
        Console.Write("Workout name: ");
        string workoutName = (Console.ReadLine() ?? "").Trim();

        if (string.IsNullOrWhiteSpace(workoutName))
        {
            Console.WriteLine("Workout name cannot be empty.");
            return;
        }

        var availableExercises = exerciseRepository.GetAll();

        if (availableExercises.Count == 0)
        {
            Console.WriteLine("No exercises available. Add exercises first.");
            return;
        }

        var workout = new Workout(workoutName);

        Console.WriteLine("===== AVAILABLE EXERCISES =====");

        while (true)
        {
            var addedExerciseNames = workout.Exercises
                .Select(ex => ex.Name)
                .ToList();

            var remaining = exerciseRepository.GetAll()
                .Where(ex => !addedExerciseNames.Any(name =>
                    name.Equals(
                        ex.Name,
                        StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (remaining.Count == 0)
            {
                Console.WriteLine("All exercises have been added to the workout.");
                break;
            }

            for (int i = 0; i < remaining.Count; i++)
            {
                Console.WriteLine(
                    $"{i + 1}. {remaining[i].Name} ({remaining[i].MuscleGroup})");
            }

            Console.Write("Choose an exercise number to add, or 'd' to finish: ");
            string input = (Console.ReadLine() ?? "").Trim();

            if (string.Equals(input, "d", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (!int.TryParse(input, out int index) ||
                index < 1 ||
                index > remaining.Count)
            {
                Console.WriteLine("Invalid exercise number.");
                continue;
            }

            var selected = remaining[index - 1];

            workout.AddExercise(selected);

            Console.WriteLine($"{selected.Name} added to {workoutName}");

            if (workout.Exercises.Count < availableExercises.Count)
            {
                while (true)
                {
                    Console.Write("Add another exercise? (y/n): ");
                    string another = (Console.ReadLine() ?? "")
                        .Trim()
                        .ToLowerInvariant();

                    if (another == "y")
                    {
                        break;
                    }

                    if (another == "n")
                    {
                        goto FinishWorkout;
                    }

                    Console.WriteLine("Invalid input. Please enter 'y' or 'n'.");
                }
            }
        }

    FinishWorkout:

        if (workout.Exercises.Count == 0)
        {
            Console.WriteLine("No exercises selected. Workout was not saved.");
            return;
        }

        workoutRepository.Add(workout);

        Console.WriteLine(
            $"Workout '{workoutName}' created with {workout.Exercises.Count} exercise(s).");
    }

    static void ViewWorkouts(IWorkoutRepository workoutRepository)
    {
        var workouts = workoutRepository.GetAll();

        if (workouts.Count == 0)
        {
            Console.WriteLine("No workouts found.");
            return;
        }

        foreach (var workout in workouts)
        {
            Console.WriteLine($"\nWorkout: {workout.Name}");

            if (workout.Exercises.Count == 0)
            {
                Console.WriteLine("  No exercises.");
                continue;
            }

            foreach (var exercise in workout.Exercises)
            {
                Console.WriteLine(
                    $"  - {exercise.Name} ({exercise.MuscleGroup})");
            }
        }
    }

    static void DeleteExercise(IExerciseRepository exerciseRepository)
    {
        while (true)
        {
            var exercises = exerciseRepository.GetAll();

            if (exercises.Count == 0)
            {
                Console.WriteLine("No exercises available to delete.");
                return;
            }

            Console.WriteLine(
                "Which exercise would you like to delete? (enter number, or 'c' to cancel)");

            for (int i = 0; i < exercises.Count; i++)
            {
                Console.WriteLine(
                    $"{i + 1}. {exercises[i].Name} ({exercises[i].MuscleGroup})");
            }

            var input = (Console.ReadLine() ?? "").Trim();

            if (string.Equals(input, "c", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return;
            }

            if (!int.TryParse(input, out int number) ||
                number < 1 || number > exercises.Count)
            {
                Console.WriteLine(
                    $"Invalid choice. Enter a number between 1 and {exercises.Count}.");
                continue;
            }

            var exerciseToRemove = exercises[number - 1];

            Console.WriteLine(
                $"Exercise selected: {exerciseToRemove.Name} ({exerciseToRemove.MuscleGroup})");

            Console.Write("Are you sure you want to delete this exercise? (y/n): ");
            var confirmation = (Console.ReadLine() ?? "").Trim();

            if (confirmation.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                exerciseRepository.Delete(exerciseToRemove);
                Console.WriteLine("Exercise deleted successfully.");
                return;
            }

            if (confirmation.Equals("n", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Deletion cancelled.");
                return;
            }

            Console.WriteLine("Invalid choice. Please try again.");
        }
    }

    static void UpdateExercise(IExerciseRepository exerciseRepository)
    {
        while (true)
        {
            var exercises = exerciseRepository.GetAll();
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

            if (!string.IsNullOrWhiteSpace(newName) &&
                exercises.Any(e =>
                e.Id != exerciseToUpdate.Id &&
                e.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"Exercise '{newName}' already exists.");
                return;
            }
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
                exerciseRepository.Update(exerciseToUpdate); // Assuming Add method updates if the exercise already exists)
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
    static void ViewFrequentlyUsedExercises(IExerciseRepository exerciseRepository)
    {
        Console.WriteLine("Show exercises used in at least how many workouts?");
        if (!int.TryParse(Console.ReadLine(), out int thresholdValue)
        || thresholdValue < 0)
        {
            Console.WriteLine("Please enter zero or a positive integer.");
            return;
        }
        var frequentlyUsedExercises = exerciseRepository.GetAll()
            .Select(ex => new {ex.Name,ex.MuscleGroup, UsageCount = ex.Workouts.Count()})
            .Where(ex => ex.UsageCount >= thresholdValue)
            .OrderByDescending(ex => ex.UsageCount).ThenBy(ex => ex.Name)
            .ToList();
        foreach (var exercise in frequentlyUsedExercises)
        {
            Console.WriteLine($"- {exercise.Name} ({exercise.MuscleGroup}): {exercise.UsageCount} workout(s)");
        }
        

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
    static void SearchExercises(IExerciseRepository exerciseRepository)
    {
        Console.Write("what do you want to search for?: ");
        string searchTerm = (Console.ReadLine() ?? "").Trim().ToLower();
        if(string.IsNullOrWhiteSpace(searchTerm))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        var exercises = exerciseRepository.GetAll();
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