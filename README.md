# CS2 HitMark

Particle hitmark with damage digits and configurable sound toggles.

## Dependencies
- Metamod:Source
- CounterStrikeSharp

## Installation
1. Build or download the plugin.
2. Copy `cs2-HitMark.dll` to `addons/counterstrikesharp/plugins/cs2-HitMark/`.
3. Start the server once to generate the config.
4. Edit the config file and restart the server.

## Config
Config file path:
`addons/counterstrikesharp/configs/plugins/cs2-HitMark/cs2-HitMark.json`

Keys:
- `version` (int)
- `disable_on_warmup` (bool)
- `mute_default_headshot_bodyshot` (bool)
- `hitmark_enabled` (bool)
- `hitmark_headshot_particle` (string)
- `hitmark_bodyshot_particle` (string)
- `hitmark_headshot_duration` (float)
- `hitmark_bodyshot_duration` (float)
- `hitmark_distance` (float)
- `hitmark_input` (string)
- `damage_digits_enabled` (bool)
- `damage_digit_particles` (array of strings, 10 entries for 0-9)
- `damage_headshot_duration` (float)
- `damage_bodyshot_duration` (float)
- `damage_distance` (float)
- `damage_spacing` (float)
- `damage_offset_x` (float)
- `damage_offset_y` (float)
- `damage_input` (string)
- `max_active_particles_per_player` (int)
- `headshot_sounds` (array of strings)
- `bodyshot_sounds` (array of strings)
- `debug` (bool)

## Commands
- `hm_toggle_hitmark` - Toggle hitmark particles for yourself.
- `hm_toggle_sound` - Toggle hitmark sounds for yourself.
