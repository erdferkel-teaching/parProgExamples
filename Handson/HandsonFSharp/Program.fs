let mapReduce2 (m : 's -> seq<'m>) (selector : 'm -> 'k) (reduce : 'k*seq<'m> -> seq<'r>) (xs : seq<'s>) =
    xs |> Seq.collect m |> Seq.groupBy selector |> Seq.collect reduce

// need no types
let mapReduce m selector reduce xs =
    xs |> Seq.collect m |> Seq.groupBy selector |> Seq.collect reduce

// make parallel
open FSharp.Collections.ParallelSeq
let mapReduceP m selector reduce xs =
    xs |> PSeq.collect m |> PSeq.groupBy selector |> PSeq.collect reduce
    
open System
open System.IO

let delimiters =
    [| for code in 0 .. 256 do 
        let c = char code
        if System.Char.IsWhiteSpace c || System.Char.IsPunctuation c then
            yield c
    |]

[<EntryPoint>]
let main argv = 
    let dirPath = @"..\..\"
    let files = Directory.EnumerateFiles dirPath 

    let split lines = 
        lines |> Seq.collect (fun (l : string) -> l.Split delimiters)

    let counts = 
        files |> mapReduceP (split << File.ReadAllLines) id (fun (k,values) -> [k, Seq.length values])

    for c in counts do printfn "%A" c
   
    0 
