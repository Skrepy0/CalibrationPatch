# CalibrationPatch

[![License: MIT](https://img.shields.io/badge/License-MIT-ff69b4.svg)](https://opensource.org/licenses/MIT)

一个为 ADOFAI 提供校准偏移自动修正功能的模组，主要用于高延迟设备，在校准后自动应用用户自定义的节拍偏移，并自动保存设置。

---

## ⚠️ 注意事项

- 本模组仅在完成游戏内校准时触发，不会持续运行或干扰正常游戏。
- **与 BetterCalibration 模组存在冲突**：两个模组会同时修改同一偏移值，可能导致偏移被反复覆盖。

## ✨ 功能特性

- **自动偏移修正**：每次游戏校准完成后自动计算并应用偏移值。
- **用户自定义节拍偏移**：支持以“拍”为单位微调延迟，主要用于高延迟设备。
- **可配置每拍毫秒数**：默认 `400` ms/拍（对应 150 BPM 即校准时用的音乐的 BPM）。

## 📜 许可证

本项目采用 [MIT License](LICENSE) 开源协议。