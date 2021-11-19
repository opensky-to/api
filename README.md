<img src="https://raw.githubusercontent.com/opensky-to/branding/master/png/OpenSkyLogo_Banner64.png" placeholder="OpenSky" />

[![Discord](https://img.shields.io/discord/837475420923756544.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.com/invite/eR3yePrj79)
[![Facebook](https://img.shields.io/badge/-OpenSky-e84393?label=&logo=facebook&logoColor=ffffff&color=6399AE&labelColor=00C2CB)](https://www.facebook.com/Opensky.to/)
![Maintained][maintained-badge]
[![Make a pull request][prs-badge]][prs]
[![License][license-badge]](LICENSE.md)

OpenSky is an open-source airline management simulation currently in development. We are actively seeking aviation enthusiast whom would love to be part of this upcoming project and shape it with us! If you have experience in coding, graphical or game design and feel like you could be an asset to the project, please head over to the [contribute page](https://www.opensky.to/contribute) and do not hesitate to jump into our [Discord](https://discord.com/invite/eR3yePrj79) and say hello! We would love to hear your ideas and feedback and are actively collecting them in our [forums](https://forum.opensky.to/)!

## OpenSky API

This repository contains the C# code for our .net Core 5.0 API. This is the public interface of the OpenSky game world server. The existing game client and agents communicate using this interface and if you want you can either interface it directly or write your own agent or client for any unsupported simulator or operating system.

The API uses OpenAPI specification version 3 (OAS3) and you can find the Swagger test interface for our test instance here:\
[https://api-dev.opensky.to/swagger/index.html](https://api-dev.opensky.to/swagger/index.html)

But please keep in mind that any data on this instance will be reset on a somewhat regular basis.

## License

Original source code and assets and present in this repository are licensed under the MIT license.

[maintained-badge]: https://img.shields.io/badge/maintained-yes-brightgreen
[license-badge]: https://img.shields.io/badge/license-MIT-blue.svg
[license]: https://github.com/maximegris/angular-electron/blob/master/LICENSE.md
[prs-badge]: https://img.shields.io/badge/PRs-welcome-red.svg
[prs]: http://makeapullrequest.com
