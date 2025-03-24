open System
open Discord.WebSocket

open CoreClient

// Feature: Audio Scrubber
// 1. Check for prefix
// 2. Check for command
// 3. Track caller and channel
// 4. Use Youtube API to find video and extract video ID
// 5. Use Youtube Explode to download audio track only using video ID
// 6. Check if caller still in channel
// 7. Create audio stream to channel

[<EntryPoint>]
let main(argv: string array) =
  let client: DiscordSocketClient = StartClient()

  let mutable loop: bool = true
  while loop do
    if Console.ReadLine() = "q" then
      client.StopAsync()
      |> Async.AwaitTask
      |> ignore
      loop <- false
  0