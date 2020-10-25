# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased

### Added

### Android App
- Textfields can now be moved vertically.
- Textfields can change colour.

### Desktop Client
- New configuration file `layout.json` that controls the layout of the Android app.

## [0.2.0] - 2020-10-24

### Added

#### Android App
- Android device now listens for new background images (as PNG) from the desktop client.

#### Desktop Client

- config option "background_image_path" to send a PNG file to the android device. The android device will then change its background to this file.
- Any exception or error is now displayed

## [0.1.0] - 2020-10-21

### Added

#### Android App

- Display for CPU and GPU temperature with fixed background image.

#### Desktop Client

- Scanning temperatures and sending them to the Client
- Small debug menu for 

[0.2.0]: https://github.com/LeslieM98/casemod-monitoring-display/releases/tag/v0.2.0
[0.1.0]: https://github.com/LeslieM98/casemod-monitoring-display/releases/tag/v0.1.0