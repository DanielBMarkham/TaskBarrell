
open System
open System
open System.IO
open Scratch
open TaskBarrellTests


// INPUT STUFF
let getStringOrEmpty (str:string []) indx = try str.[indx] with |_->""
let getFloatOrEmpty (str:string []) indx = try Double.Parse(str.[indx]) with |_->0.0

let processInputLine:ProcessInputLineFunction =
  (fun strippedSourceFileName fileLine ->
    try // The file line reads, but does not parse
      let lineSplit=fileLine.Split('\t',4)
      let userstory=getStringOrEmpty lineSplit 0
      let taskname=getStringOrEmpty lineSplit 1
      let hoursworkedsofar=getFloatOrEmpty lineSplit 2
      let estimatedhoursremainingtofinish=getFloatOrEmpty lineSplit 3
      Some {
        DeveloperName=strippedSourceFileName
        UserStory=userstory
        TaskName=taskname
        HoursWorkedSoFar=hoursworkedsofar
        EstimatedHoursRemainingToFinish=estimatedhoursremainingtofinish
      }
    with |_->None
  )

let transformFileLines:TransformFileLinesFunction =
  (fun strippedSourceFileName fileLines->
    let ret:seq<FlattenedTask option> = 
      fileLines |> Seq.map(fun line->
        processInputLine strippedSourceFileName line
        )
    ret  
  )
let processInputFile:ProcessInputFileFunction =
  (fun fileName->
    try // the entire file fails the read
      let dotFound = if fileName.LastIndexOf(".") = -1 then 0 else fileName.LastIndexOf(".")
      let lastFileSeparator = if fileName.LastIndexOf(Path.DirectorySeparatorChar) =(-1) then -1 else fileName.LastIndexOf(Path.DirectorySeparatorChar)
      let strippedSourceFileName = fileName.Substring(lastFileSeparator+1,dotFound-lastFileSeparator-1)

      let fileLines= File.ReadAllLines(fileName)
      let transformedFileLines = 
        transformFileLines strippedSourceFileName fileLines
        |> Seq.choose(fun line->line)
      Some (strippedSourceFileName, transformedFileLines)
    with |_->None
  )
let processInput (argv:string []):CleanedProgramInput =
  // step 1: pull in command parms and any OS data they point to
  let reportFilename= (DateTime.Today.ToShortDateString()).Replace("/","-") + "-report.csv"
  let sourceDirectory=try argv.[1] with |_->Environment.CurrentDirectory + string Path.DirectorySeparatorChar
  let targetDirectory=try argv.[2] with |_->Environment.CurrentDirectory + string Path.DirectorySeparatorChar
  let sourceFiles=try Directory.GetFiles(sourceDirectory, "*.txt") with |_->[||]
  // step 2: process each incoming file/hunk of input data
  let processedInputFiles = 
    try // catch something if the entire list read fails
      sourceFiles |> Seq.map(fun eachFile->processInputFile eachFile)
        |> Seq.choose(fun possibleFile->possibleFile)
    with |_->Seq.empty
  {
    ReportFileName=reportFilename
    SourceDirectory=sourceDirectory
    TargetDirectory=targetDirectory
    SourceFileContents=processedInputFiles
  }



// IMPORTANT BUSINESS WORK
let processData:ProcessDataFunction = 
  (fun cleanedInputData->
  let bigOldFlatFileOfTasks=
    cleanedInputData.SourceFileContents
    |>Seq.collect(fun (eachFileComingIn:string*seq<FlattenedTask>)->
    snd eachFileComingIn
    )  
  // now that we have the data flattened, we can slice and dice it
  let tasksNotStartedOnYet = bigOldFlatFileOfTasks |> Seq.filter(fun task->task.HoursWorkedSoFar<=0.0)
  let tasksStarted = bigOldFlatFileOfTasks |> Seq.filter(fun task->task.HoursWorkedSoFar>0.0)
  let tasksNotStartedGroupedByUserStory = tasksNotStartedOnYet |> Seq.sortBy(fun task->task.UserStory)
  let tasksStartedGroupedByUserStory = tasksStarted |> Seq.sortBy(fun task->task.UserStory)
  (cleanedInputData,tasksNotStartedGroupedByUserStory,tasksStartedGroupedByUserStory)
  )

// OUTPUT STUFF
let processOutput:ProcessOutputFunction = 
  (fun (cleanedInputData, tasksNotStartedGroupedByUserStory, tasksStartedGroupedByUserStory) ->
    try
      let targetFileName = cleanedInputData.TargetDirectory + "" + cleanedInputData.ReportFileName
      printfn "%s" targetFileName
      let sbOuputText = Text.StringBuilder(65536)
      let convertFlattenedTaskToTextLine (task:FlattenedTask) (stringBuilderOutput:Text.StringBuilder)  =
        stringBuilderOutput.AppendFormat (" {0} \t {1} \t {2} \t {3} \t {4} \r\n", task.DeveloperName, task.UserStory, task.TaskName, string task.HoursWorkedSoFar, string task.EstimatedHoursRemainingToFinish) |> ignore
        ()

      sbOuputText.Append ("TASKS NOT STARTED" + "\r\n") |> ignore
      tasksNotStartedGroupedByUserStory |> Seq.iter(fun tsk->convertFlattenedTaskToTextLine tsk sbOuputText)
      sbOuputText.Append ("\r\n\TASKS STARTED\r\n") |> ignore
      tasksStartedGroupedByUserStory |> Seq.iter(fun tsk->convertFlattenedTaskToTextLine tsk sbOuputText)
      File.WriteAllText(targetFileName, string sbOuputText)
      ()
    with |_->() |> ignore
    0  
  )

[<EntryPoint>]
let main argv =
    printfn "Hello World from TaskBarrell!"
    argv |> processInput |> processData |> processOutput



// let x=seq{0..100}
//     |> Seq.iter(fun x->
//     (match (x%3=0, x%5=0) with
//     |(true,true)->printfn "%s" "FizzBuzz"
//     |(true,_)->printfn "%s" "Fizz"
//     |(_,true)->printfn "%s""Buzz"
//     |(_,_)->())) 
