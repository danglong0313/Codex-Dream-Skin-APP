# Open Source and Attribution Notice

This repository is a derivative of [Fei-Away/Codex-Dream-Skin](https://github.com/Fei-Away/Codex-Dream-Skin). Upstream attribution and a direct project link are retained.

## License scope

- Original Windows Studio UI source files under `windows/studio/` are provided under the MIT License in `windows/studio/LICENSE`, except for `engine/`, bundled themes, images, and generated build output.
- The bundled engine under `windows/studio/engine/` is derived from the upstream implementation and remains outside the scope of the Studio UI MIT license.
- The upstream repository does not currently publish a repository-wide root license. Public source availability alone does not grant permission to copy, modify, redistribute, sublicense, or commercialize upstream files.
- Files without an explicit license remain subject to the rights of their respective copyright holders.

The MIT licenses in this repository do not relicense upstream code or third-party assets outside their stated scope.

## Trademarks and character artwork

OpenAI, Codex, Hatsune Miku, and related names, logos, trademarks, characters, and artwork belong to their respective rights holders. This project is unofficial and is not endorsed by or affiliated with OpenAI or the relevant character rights holders.

Theme previews and character imagery are supplied for non-commercial demonstration and customization. Verify the necessary rights before redistribution or commercial use. Rights holders may request removal through the repository issue tracker.

## Security model

The application uses loopback CDP to apply a visual layer to the Codex desktop renderer. It does not modify WindowsApps, `app.asar`, official application binaries, or signatures. It does not grant any license to the Codex application itself.
