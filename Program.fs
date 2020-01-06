
open System
open Scratch
open TaskBarrellTests


// app
type TaskBarellCommandLine=string
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

[<EntryPoint>]
let main argv =
    printfn "Hello World from TaskBarrell!"
    0