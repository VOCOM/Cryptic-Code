module CoreClient

open System
open System.IO
open System.Threading.Tasks
open System.Reflection

open Discord
open Discord.WebSocket

// Environment
let private env = "Cryptic_Code..env"
let private assembly = Assembly.GetExecutingAssembly()
let private aStream = assembly.GetManifestResourceStream(env)
let private aReader = new StreamReader(aStream)
let private aContent = aReader.ReadToEnd()

// Callbacks
let private connectCallback(): Task = Task.Run(fun () -> printfn "Connected...")
let private readyCallback(): Task = Task.Run(fun () -> printfn "Ready...")
let private messageCallback(msg: SocketMessage): Task = 
  task{
    let channel = msg.Channel
    let channelType = channel.ChannelType.ToString()
    printfn "Read %s from %s which is a %s" msg.Content channel.Name channelType
  }
  
let private token: string =
    let index = aContent.IndexOf("discord-token")
    if index >= 0 then
      let startIndex = aContent.IndexOf("\"") + 1
      let endIndex = aContent.IndexOf("\"", startIndex)
      aContent.Substring(startIndex, endIndex - startIndex)
    else
      String.Empty
let private config: DiscordSocketConfig = 
    let config = DiscordSocketConfig()
    config.GatewayIntents <- config.GatewayIntents ||| GatewayIntents.MessageContent
    config

let StartClient(): DiscordSocketClient =
  let client:DiscordSocketClient = new DiscordSocketClient(config)
  
  // Attach Callbacks
  client.add_Connected(new Func<Task>(connectCallback))
  client.add_Ready(new Func<Task>(readyCallback))
  client.add_MessageReceived(new Func<SocketMessage, Task>(messageCallback))
  
  // Initialization sequence
  let tasks = [
    client.LoginAsync(TokenType.Bot,token);
    client.StartAsync()
  ]
  
  // Execute initialization
  tasks
  |> List.map Async.AwaitTask
  |> ignore

  client
