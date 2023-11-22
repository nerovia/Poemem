using Poemem;
using Poemem.Quiz;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;

var cache = new Dictionary<(string, Source), Poem>();

var queryArgument = new Argument<string>("query");

var rangeOption = new Option<Range>(new[] { "--range", "-r" }, parseArgument: Parsing.ParseRange);
rangeOption.SetDefaultValue(..);

var modeOption = new Option<Mode>(new[] { "--mode", "-m" });
modeOption.SetDefaultValue(Mode.Blanks);

var sourceOption = new Option<Source>(new[] { "--source", "-s" });
sourceOption.SetDefaultValue(Source.Local);

var difficultyOption = new Option<Difficulty>(new[] { "--difficulty", "-d" });
difficultyOption.SetDefaultValue(Difficulty.Medium);

var quizCommand = new Command("quiz")
{
	queryArgument,
	sourceOption,
	rangeOption,
	modeOption,
	difficultyOption,
};
quizCommand.SetHandler(HandleQuiz, queryArgument, sourceOption, rangeOption, modeOption, difficultyOption);

var fetchCommand = new Command("fetch")
{
	queryArgument,
	rangeOption,
	sourceOption,
};
fetchCommand.SetHandler(HandleFetch, queryArgument, sourceOption, rangeOption);

var exitCommand = new Command("exit");
exitCommand.AddAlias("quit");
exitCommand.SetHandler(() => Environment.Exit(0));
exitCommand.IsHidden = true;

var rootCommand = new RootCommand() { quizCommand, fetchCommand, exitCommand };
rootCommand.SetHandler(HandleRoot);
rootCommand.Invoke(args);

async Task HandleRoot()
{
	exitCommand.IsHidden = false;
	while (true)
	{
		Console.Write("poemem > ");
		var input = Console.ReadLine();
		if (input == null)
			return;
		await rootCommand.InvokeAsync(input);
	}
}

async Task HandleQuiz(string query, Source source, Range range, Mode mode, Difficulty difficulty)
{
	Debug.WriteLine($"Handle Quiz: { new { query, source, range, mode, difficulty } }");

	IPoem poem = await FindPoem(query, source);

	IPoemQuiz quiz = mode switch
	{
		Mode.Blanks => new WordQuiz(),
		Mode.Lines => new LineQuiz(),
		_ => throw new ArgumentException()
	};

	var options = new QuizOptions(poem, range, difficulty)
	{
		Substitution = it => it.Replace("ß", "ss")
	};

	WriteTitle(poem);

	IQuizResult result = quiz.Execute(options) ?? new QuizResult("Quiz canceled :(");

	WriteResult(result);
}

async Task HandleFetch(string query, Source source, Range range)
{
	Debug.WriteLine($"Handle Fetch: { new { source, query, range } }");

	var poem = await FindPoem(query, source);

	WriteTitle(poem);

	foreach (var verse in poem.Verses[range])
	{
		foreach (var line in verse)
			Line.Current.Write(line).NewLine();
		Line.Current.NewLine();
	}
}

async Task<Poem> FindPoem(string query, Source source)
{
	if (cache.TryGetValue((query, source), out var poem))
	{
		Debug.WriteLine("Found poem in cache");
		return poem;
	}

	poem = source switch
	{
		Source.Local => await PoemService.FetchFromLocal(query),
		Source.PoetryDB => await PoemService.FetchFromPoetryDB(query),
		_ => throw new ArgumentException()
	};

	cache.Add((query, source), poem);

	return poem;
}

void WriteTitle(IPoem poem)
{
	Line.Current
		.NewLine()
		.Style(33)
		.Write(poem.Title)
		.Style(0)
		.NewLine(2);
}

void WriteResult(IQuizResult result)
{
	Line.Current
		.Style(33)
		.Write(result)
		.Style(0)
		.NewLine(2);
}

enum Mode
{
	Blanks,
	Lines,
}

enum Difficulty
{
	Easy,
	Medium,
	Hard,
	Extreme,
}

enum Source
{
	Local,
	PoetryDB
}
