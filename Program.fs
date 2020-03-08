
open System
open System
open System.IO
open Scratch
open TaskBarrellTests


// let x=seq{0..100}
//     |> Seq.iter(fun x->
//     (match (x%3=0, x%5=0) with
//     |(true,true)->printfn "%s" "FizzBuzz"
//     |(true,_)->printfn "%s" "Fizz"
//     |(_,true)->printfn "%s""Buzz"
//     |(_,_)->())) 


// app
(*type TaskBarellCommandLine=string
type TaskBarrellApp=TaskBarellCommandLine->int

// incoming
type IncomingChunks=
  {
    File:System.IO.FileInfo
    FileContents:string list
  }

type TaskBarrelIncomingData<'a>=seq<'a>->int
type TaskInputFile = {FileName:string}
type Tasks = {lines:seq<string>}
type ProcessIncomingTasks=TaskInputFile->Tasks
let readTasksIn:ProcessIncomingTasks =
  (fun fileName->
  let ret:seq<string>=Seq.empty
  {lines=ret}
  )

// outgoing
type TaskOutputOutgoing =
  {
    FileName:string
    TasksToWrite:Tasks
  }
type ProcessOutgoingTasks=TaskOutputOutgoing->unit
let writeTasksOut:ProcessOutgoingTasks =
  (fun outParms->
    //outParms.
    ()
  )
*)
type Task =
  {
    UserStory:string
    TaskName:string
    HoursWorkedSoFar:float
    EstimatedHoursRemainingToFinish:float
  }
type CleanedProgramInput =
  {
    ReportFileName:string
    SourceDirectory:string
    TargetDirectory:string
    SourceFileContents:seq<string *seq<Task>>
  }


let processInput (argv:string []) =
  let reportFilename= (DateTime.Today.ToShortDateString()).Replace("/","-") + "-report.csv"
  let sourceDirectory=try argv.[1] with |_->Environment.CurrentDirectory + string Path.DirectorySeparatorChar
  let targetDirectory=try argv.[2] with |_->Environment.CurrentDirectory + string Path.DirectorySeparatorChar
  let sourceFiles=try Directory.GetFiles(sourceDirectory, "*.txt") with |_->[||]
  let getStringOrEmpty (str:string []) indx = try str.[indx] with |_->""
  let getFloatOrEmpty (str:string []) indx = 
    try 
      Double.Parse(str.[indx])
    with |_->0.0
  let sourceFileContents=
    sourceFiles
    |> Seq.map(fun x->
      let dotFound = if x.LastIndexOf(".") = -1 then 0 else x.LastIndexOf(".")
      let lastFileSeparator = if x.LastIndexOf(Path.DirectorySeparatorChar) =(-1) then -1 else x.LastIndexOf(Path.DirectorySeparatorChar)
      let strippedSourceFileName = x.Substring(lastFileSeparator+1,dotFound-lastFileSeparator-1)
      (strippedSourceFileName,
        try
          let fileLines=File.ReadAllLines(x)
          let transformedFileLines = fileLines |> Seq.map(fun y->
            let lineSplit=y.Split('\t',4)
            let userstory=getStringOrEmpty lineSplit 0
            let taskname=getStringOrEmpty lineSplit 1
            let hoursworkedsofar=getFloatOrEmpty lineSplit 2
            let estimatedhoursremainingtofinish=getFloatOrEmpty lineSplit 3
            {
              UserStory=userstory
              TaskName=taskname
              HoursWorkedSoFar=hoursworkedsofar
              EstimatedHoursRemainingToFinish=estimatedhoursremainingtofinish
            }
            )
          transformedFileLines
        with |_->Seq.empty)
      )

  {
    ReportFileName=reportFilename
    SourceDirectory=sourceDirectory
    TargetDirectory=targetDirectory
    SourceFileContents=sourceFileContents
  }
type FlattenedTask =
    {
      DeveloperName:string;
      UserStory:string;
      TaskName:string;
      HoursWorkedSoFar:float;
      EstimatedHoursRemainingToFinish:float
    }
let processData (cleanedInputData:CleanedProgramInput) =
  let bigOldFlatFileOfTasks=
    cleanedInputData.SourceFileContents
    |>Seq.collect(fun (eachFileComingIn:string*seq<Task>)->
      (snd eachFileComingIn
      |> Seq.map(fun (eachLineComingIn:Task)->
        {
          DeveloperName=fst eachFileComingIn
          UserStory=eachLineComingIn.UserStory
          TaskName=eachLineComingIn.TaskName
          HoursWorkedSoFar=eachLineComingIn.HoursWorkedSoFar
          EstimatedHoursRemainingToFinish=eachLineComingIn.EstimatedHoursRemainingToFinish
        }
        )
      )
    ) |> Seq.filter(fun x->(x.UserStory.Trim().Length+x.TaskName.Trim().Length>0)) // no user story or task name, ignore

  // now that we have the data flattened, we can slice and dice it
  let tasksNotStartedOnYet = bigOldFlatFileOfTasks |> Seq.filter(fun task->task.HoursWorkedSoFar<=0.0)
  let tasksStarted = bigOldFlatFileOfTasks |> Seq.filter(fun task->task.HoursWorkedSoFar>0.0)
  let tasksNotStartedGroupedByUserStory = tasksNotStartedOnYet |> Seq.sortBy(fun task->task.UserStory)
  let tasksStartedGroupedByUserStory = tasksStarted |> Seq.sortBy(fun task->task.UserStory)
  (cleanedInputData,tasksNotStartedGroupedByUserStory,tasksStartedGroupedByUserStory)

let processOutput (cleanedInputData,tasksNotStartedGroupedByUserStory,tasksStartedGroupedByUserStory) =

(*  printfn "\nTasks Not Started"
  printfn "-----------------"
  tasksNotStartedGroupedByUserStory |> Seq.iter(fun x->printfn "%s\t--\t%s--\t%s" x.UserStory x.TaskName (string x.HoursWorkedSoFar))
  printfn "\nTasks Started"
  printfn "-----------------"
  tasksStartedGroupedByUserStory |> Seq.iter(fun x->printfn "%s\t--\t%s--\t%s" x.UserStory x.TaskName (string x.HoursWorkedSoFar))
*)
  try
    let targetFileName = cleanedInputData.TargetDirectory + "" + cleanedInputData.ReportFileName
    printfn "%s" targetFileName
    let sbOuputText = Text.StringBuilder(65536)
    let convertFlattenedTaskToTextLine (task:FlattenedTask) (stringBuilderOutput:Text.StringBuilder)  =
      stringBuilderOutput.AppendFormat (" {0} \t {1} \t {2} \t {3} \t {4} \r\n", task.DeveloperName, task.UserStory, task.TaskName, string task.HoursWorkedSoFar, string task.EstimatedHoursRemainingToFinish) |> ignore
      //printfn "%s" (stringBuilderOutput.ToString())
      ()

    sbOuputText.Append ("TASKS NOT STARTED" + "\r\n") |> ignore
    tasksNotStartedGroupedByUserStory |> Seq.iter(fun tsk->convertFlattenedTaskToTextLine tsk sbOuputText)
    sbOuputText.Append ("\r\n\TASKS STARTED\r\n") |> ignore
    tasksStartedGroupedByUserStory |> Seq.iter(fun tsk->convertFlattenedTaskToTextLine tsk sbOuputText)

    //printfn "----"
    File.WriteAllText(targetFileName, string sbOuputText)
    //let buffTxt = sbOuputText.ToString()
    //printfn "%s" buffTxt
    //printfn "----"
    ()
  with |_->() |> ignore
  0



[<EntryPoint>]
let main argv =
    printfn "Hello World from TaskBarrell!"
    argv |> processInput |> processData |> processOutput

