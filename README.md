# Azuser - Azure SQL Server User Management

![Azuser](https://github.com/Inzanit/azuser/blob/master/.resources/ReadMeIntro.gif?raw=true)

## Motivation

Pronounced As-user, Azuser eliminates the need for using a command line interface to manage SQL Server users and logins. It is currently not possible to easily manage users for databases through a GUI like SQL Server Management Studio (SSMS) - user management is achieved through the CLI with verbose options to configure users' roles for a given database. Azuser is a lightweight desktop application written in WPF, aimed at completely removing the need for any command line interface, ensuring database users have and get the right access.

## What does it do

- Add, edit and drop logins from your SQL Server database
- Add, edit and drop users for logins from your SQL Server database

Azuser automatically manages users for logins on your server.

**Azuser works for both Azure and non-Azure SQL Server databases.**

## User guide

You can download Azuser from [here](http://inzanit.com/Releases/Azuser/Setup.exe). It will automatically update on every restart (should there be an update).

## Spot a bug, issue or something else?

Feel free to [submit an issue](https://github.com/Inzanit/azuser/issues/new) via this repository, on GitHub. Contributions are also welcome.

## Do with it what you want

Azuser is licensed under MIT, you're free to do with it what you want. Contribute, download it, fork it, steal it, sell it, repurpose it.

## Roadmap

This is a small personal project, and is maintained during personal time - timelines will be rough and may not be exact.

| Feature                     | ETA      | Issue link                                               |
|-----------------------------|----------|----------------------------------------------------------|
| Theme switching (dark mode) | Dec 2018 | [Link](https://github.com/Inzanit/azuser/issues/2)       |
| Browse users by database    | Jan 2019 | [Link](https://github.com/Inzanit/azuser/issues/3)       |
| Release standalone package  | 2019     | [Link](https://github.com/Inzanit/azuser/issues/4)       |
| Multiple connections        | 2019     | [Link](https://github.com/Inzanit/azuser/issues/5)       |

## Stack

Azuser is split into two projects, the desktop client and the database management project.

The desktop client is written in WPF, using the MVVM architecture, targeting .NET Framework 4.7, using C#7. A number of small, reliable, dependencies have been added:

- [Mahapps Metro](https://github.com/MahApps/MahApps.Metro) for styling and controls
- [Serilog](https://github.com/serilog/serilog) for logging
- [Autofac](https://github.com/autofac/Autofac) as the IoC container
- [Squirrel](https://github.com/Squirrel/Squirrel.Windows) for the installer framework
- [MVVMLight](https://github.com/lbugnion/mvvmlight) as the MVVM framework

The second project, `Azuser.Services` is where the magic happens. This project targets .NET Standard 2.0 and is fully capable of standing alone, without the desktop client. This project has no dependencies other than the .NET libraries for the SQL client.
