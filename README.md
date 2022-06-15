# Blakserv Logbot

Blakserv Logbot tails blakserv (Meridian 59 server) log files, queues log
entries and sends them to a Discord channel. Functional, but a work in
progress. Agnostic to the Meridian 59 server type - this program basically
just grabs new lines in any specified text file and outputs them to Discord.

## Build

Requires Visual Studio 2022 and .NET 6.0. The Publish configuration file has
been included to make single file publishing easier.

## Run

The program generates a config.json on first run. You will need to edit this
file and add in your Discord bot token, Discord channel ID and the paths for
which folders and files you wish to tail. By default the program will send
one log entry per second to Discord, and queue a maximum of 20 log entries
before discarding new ones.

## Contributing

Contributions welcome, but please emulate any surrouding code style.

## License

Code released under the [the MIT license](https://github.com/skittles1/blakserv_logbot/blob/master/LICENSE).
