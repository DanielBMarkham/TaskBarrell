// F#
module TaskBarrellTests
open System 
//open Expecto

type FlattenedTask =
  {
    DeveloperName:string;
    UserStory:string;
    TaskName:string;
    HoursWorkedSoFar:float;
    EstimatedHoursRemainingToFinish:float
  }
type CleanedProgramInput =
  {
    ReportFileName:string
    SourceDirectory:string
    TargetDirectory:string
    SourceFileContents:seq<string *seq<FlattenedTask>>
  }
type ProcessInputLineFunction =
  string->string->FlattenedTask option
type ProcessInputFileFunction =
  string->(string*seq<FlattenedTask>) option
type ProcessDataOutput = 
  CleanedProgramInput * seq<FlattenedTask> * seq<FlattenedTask>
type ProcessDataFunction = 
  CleanedProgramInput -> ProcessDataOutput

type TransformFileLinesFunction =
  string->string[]->seq<FlattenedTask option>
type ProcessInputFunction =
  string[]->CleanedProgramInput
type ProcessOutputFunction =
  CleanedProgramInput * seq<FlattenedTask> * seq<FlattenedTask> -> int



(*[<Tests>]
let tests =
  testList "Incoming File Stream Works" [
  testCase "Empty file gets bookends" <| fun _ ->
    Expect.isTrue (true) "Empty input produces FileEnd bookend"  
  ]*)