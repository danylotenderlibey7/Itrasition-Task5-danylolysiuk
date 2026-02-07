using System;
using System.IO;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Task5.Audio.Interfaces;
using Task5.Determinism;

namespace Task5.Audio
{
    public class WavAudioGenerator : IAudioGenerator
    {
        public byte[] GeneratePreviewWav(string songId, string locale, int durationSeconds = 30)
        {
            locale = string.IsNullOrWhiteSpace(locale) ? "en-US" : locale.Trim();

            if (!SongId.TryParseSongId(songId, out ulong seed, out int index) || index <= 0)
                throw new ArgumentException("Invalid songId. Expected <seed>-<index>", nameof(songId));

            int detSeed = DeterministicSeed.MakeDetSeed(seed, index, locale);
            var rnd = new Random(detSeed);

            const int sampleRate = 44100;
            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            var genre = PickGenre(index, rnd);
            var plan = BuildPlan(genre, rnd);

            var mixer = new MixingSampleProvider(waveFormat) { ReadFully = true };

            AddHarmony(mixer, plan, waveFormat, durationSeconds, rnd);
            AddBass(mixer, plan, waveFormat, durationSeconds, rnd);
            AddMelody(mixer, plan, waveFormat, durationSeconds, rnd);
            AddDrums(mixer, plan, waveFormat, durationSeconds, rnd);

            return RenderToWav16(mixer, waveFormat, durationSeconds, plan.Master);
        }

        private enum Genre { Ambient, LoFi, SynthPop }
        private enum Waveform { Sine, Triangle, Saw }

        private static Genre PickGenre(int index, Random rnd)
        {
            var v = (index + rnd.Next(0, 7)) % 3;
            return (Genre)v;
        }

        private sealed class Plan
        {
            public Genre Genre;
            public int Bpm;
            public double Beat;
            public double Bar;
            public int RootMidi;
            public int[] Scale;
            public int[][] ProgressionDegrees;

            public float Master;
            public float Pad;
            public float Bass;
            public float Melody;
            public float Drums;

            public int SwingPercent;
            public int PatternId;
            public int DrumStyle;
            public int MelodyStyle;
            public int BassStyle;
        }

        private static Plan BuildPlan(Genre genre, Random rnd)
        {
            bool minor = rnd.NextDouble() < 0.48;

            int rootMidi = 45 + rnd.Next(0, 18);

            int[] major = { 0, 2, 4, 5, 7, 9, 11 };
            int[] minorNat = { 0, 2, 3, 5, 7, 8, 10 };

            int[][] progMajor1 = { new[] { 1, 3, 5 }, new[] { 5, 7, 2 }, new[] { 6, 1, 3 }, new[] { 4, 6, 1 } }; // I V vi IV
            int[][] progMajor2 = { new[] { 1, 3, 5 }, new[] { 6, 1, 3 }, new[] { 4, 6, 1 }, new[] { 5, 7, 2 } }; // I vi IV V
            int[][] progMajor3 = { new[] { 4, 6, 1 }, new[] { 1, 3, 5 }, new[] { 5, 7, 2 }, new[] { 6, 1, 3 } }; // IV I V vi

            int[][] progMinor1 = { new[] { 1, 3, 5 }, new[] { 7, 2, 4 }, new[] { 6, 1, 3 }, new[] { 7, 2, 4 } }; // i VII VI VII
            int[][] progMinor2 = { new[] { 1, 3, 5 }, new[] { 6, 1, 3 }, new[] { 3, 5, 7 }, new[] { 7, 2, 4 } }; // i VI III VII
            int[][] progMinor3 = { new[] { 1, 3, 5 }, new[] { 4, 6, 1 }, new[] { 7, 2, 4 }, new[] { 3, 5, 7 } }; // i iv VII III

            int[][] progression = minor
                ? (rnd.Next(0, 3) switch { 0 => progMinor1, 1 => progMinor2, _ => progMinor3 })
                : (rnd.Next(0, 3) switch { 0 => progMajor1, 1 => progMajor2, _ => progMajor3 });

            var plan = new Plan
            {
                Genre = genre,
                RootMidi = rootMidi,
                Scale = minor ? minorNat : major,
                ProgressionDegrees = progression,
                PatternId = rnd.Next(0, 6),
                DrumStyle = rnd.Next(0, 4),
                MelodyStyle = rnd.Next(0, 5),
                BassStyle = rnd.Next(0, 4)
            };

            switch (genre)
            {
                case Genre.Ambient:
                    plan.Bpm = rnd.Next(60, 106);
                    plan.SwingPercent = 0;
                    plan.Master = 0.28f;
                    plan.Pad = 0.45f;
                    plan.Bass = 0.10f;
                    plan.Melody = 0.12f;
                    plan.Drums = 0.06f;
                    break;

                case Genre.LoFi:
                    plan.Bpm = rnd.Next(70, 111);
                    plan.SwingPercent = rnd.Next(10, 26);
                    plan.Master = 0.30f;
                    plan.Pad = 0.18f;
                    plan.Bass = 0.28f;
                    plan.Melody = 0.16f;
                    plan.Drums = 0.25f;
                    break;

                default:
                    plan.Bpm = rnd.Next(95, 151);
                    plan.SwingPercent = rnd.Next(0, 14);
                    plan.Master = 0.32f;
                    plan.Pad = 0.14f;
                    plan.Bass = 0.26f;
                    plan.Melody = 0.26f;
                    plan.Drums = 0.28f;
                    break;
            }

            plan.Beat = 60.0 / plan.Bpm;
            plan.Bar = plan.Beat * 4.0;

            return plan;
        }

        private static void AddHarmony(MixingSampleProvider mixer, Plan p, WaveFormat wf, int durationSeconds, Random rnd)
        {
            for (double barStart = 0, bar = 0; barStart < durationSeconds; barStart += p.Bar, bar++)
            {
                var triad = p.ProgressionDegrees[(int)bar % p.ProgressionDegrees.Length];

                int m1 = DegreeToMidi(p, triad[0], 0);
                int m2 = DegreeToMidi(p, triad[1], 0);
                int m3 = DegreeToMidi(p, triad[2], 0);

                double len = p.Genre == Genre.Ambient ? p.Bar * 1.1 : p.Bar;

                var chord = new MixingSampleProvider(wf) { ReadFully = true };

                if (p.Genre == Genre.Ambient)
                {
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m1), Waveform.Triangle, barStart, len, p.Pad * 0.65f, 0.50, 0.25, 0.85, 0.55));
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m2), Waveform.Triangle, barStart, len, p.Pad * 0.45f, 0.50, 0.25, 0.85, 0.55));
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m3), Waveform.Triangle, barStart, len, p.Pad * 0.38f, 0.50, 0.25, 0.85, 0.55));
                    if (rnd.NextDouble() < 0.6)
                        chord.AddMixerInput(MakeNote(wf, MidiToFreq(m1) * 2, Waveform.Sine, barStart, len, p.Pad * 0.10f, 0.35, 0.20, 0.60, 0.55));
                }
                else
                {
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m1), Waveform.Triangle, barStart, len, p.Pad * 0.55f, 0.10, 0.15, 0.70, 0.20));
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m2), Waveform.Triangle, barStart, len, p.Pad * 0.38f, 0.10, 0.15, 0.70, 0.20));
                    chord.AddMixerInput(MakeNote(wf, MidiToFreq(m3), Waveform.Triangle, barStart, len, p.Pad * 0.32f, 0.10, 0.15, 0.70, 0.20));
                    if (p.Genre == Genre.SynthPop && rnd.NextDouble() < 0.4)
                        chord.AddMixerInput(MakeNote(wf, MidiToFreq(m1) * 2, Waveform.Saw, barStart, len * 0.5, p.Pad * 0.08f, 0.01, 0.04, 0.50, 0.12));
                }

                mixer.AddMixerInput(chord);
            }
        }

        private static void AddBass(MixingSampleProvider mixer, Plan p, WaveFormat wf, int durationSeconds, Random rnd)
        {
            double beat = p.Beat;

            int[] rhythm = p.BassStyle switch
            {
                0 => new[] { 1, 0, 1, 0 },
                1 => new[] { 1, 0, 1, 1 },
                2 => new[] { 1, 1, 0, 1 },
                _ => new[] { 1, 0, 0, 1 }
            };

            for (double t = 0; t < durationSeconds; t += beat)
            {
                int stepInBar = (int)Math.Floor((t % p.Bar) / beat);
                if (rhythm[stepInBar % 4] == 0 && rnd.NextDouble() < 0.8) continue;

                int barIndex = (int)Math.Floor(t / p.Bar);
                var triad = p.ProgressionDegrees[barIndex % p.ProgressionDegrees.Length];

                int degree = triad[0];
                if (p.Genre == Genre.SynthPop && stepInBar == 2) degree = triad[rnd.Next(0, 3)];
                if (p.Genre == Genre.LoFi && stepInBar == 3) degree = triad[1];

                if (rnd.NextDouble() < 0.12) degree = rnd.Next(1, 8);

                int midi = DegreeToMidi(p, degree, -12);
                double f = MidiToFreq(midi);

                (double a, double d, double s, double r) = p.Genre switch
                {
                    Genre.LoFi => (0.01, 0.09, 0.65, 0.14),
                    Genre.SynthPop => (0.01, 0.06, 0.60, 0.12),
                    _ => (0.03, 0.12, 0.55, 0.25)
                };

                var wfType = p.Genre == Genre.SynthPop ? Waveform.Sine : Waveform.Triangle;

                double len = beat * (p.Genre == Genre.SynthPop ? 0.95 : 0.9);
                mixer.AddMixerInput(MakeNote(wf, f, wfType, t, len, p.Bass * 0.85f, a, d, s, r));
            }
        }

        private static void AddMelody(MixingSampleProvider mixer, Plan p, WaveFormat wf, int durationSeconds, Random rnd)
        {
            if (p.Genre == Genre.Ambient)
            {
                double step = p.Beat * (rnd.NextDouble() < 0.5 ? 2.0 : 1.0);
                for (double t = 0; t < durationSeconds; t += step)
                {
                    if (rnd.NextDouble() < 0.25) continue;

                    int barIndex = (int)Math.Floor(t / p.Bar);
                    var triad = p.ProgressionDegrees[barIndex % p.ProgressionDegrees.Length];

                    int degree = rnd.NextDouble() < 0.75 ? triad[rnd.Next(0, 3)] : rnd.Next(1, 8);
                    int midi = DegreeToMidi(p, degree, 12 + (rnd.NextDouble() < 0.25 ? 12 : 0));
                    double f = MidiToFreq(midi);

                    double len = step * (rnd.NextDouble() < 0.6 ? 1.5 : 1.0);

                    mixer.AddMixerInput(MakeNote(wf, f, Waveform.Sine, t, len, p.Melody * 0.85f, 0.12, 0.20, 0.30, 0.55));
                }
                return;
            }

            double[] stepOptions = p.Genre == Genre.LoFi
                ? new[] { p.Beat / 2.0, p.Beat }
                : new[] { p.Beat / 4.0, p.Beat / 2.0, p.Beat };

            double time = 0;
            int stepIndex = 0;

            while (time < durationSeconds)
            {
                double stepDur = stepOptions[rnd.Next(stepOptions.Length)];
                double nominal = time;
                double start = ApplySwing(p, nominal, stepDur, stepIndex);

                int barIndex = (int)Math.Floor(start / p.Bar);
                var triad = p.ProgressionDegrees[barIndex % p.ProgressionDegrees.Length];

                bool rest = p.MelodyStyle switch
                {
                    0 => rnd.NextDouble() < 0.20,
                    1 => rnd.NextDouble() < 0.30,
                    2 => rnd.NextDouble() < 0.12,
                    3 => rnd.NextDouble() < 0.25,
                    _ => rnd.NextDouble() < 0.18
                };

                if (!rest)
                {
                    int degree;
                    double pick = rnd.NextDouble();

                    if (pick < 0.65) degree = triad[rnd.Next(0, 3)];
                    else if (pick < 0.85) degree = rnd.Next(1, 8);
                    else degree = triad[0];

                    int octave = p.Genre == Genre.SynthPop ? 24 : 12;
                    if (rnd.NextDouble() < 0.20) octave += 12;

                    int midi = DegreeToMidi(p, degree, octave);
                    double f = MidiToFreq(midi);

                    var wfType = p.Genre == Genre.SynthPop ? (rnd.NextDouble() < 0.65 ? Waveform.Saw : Waveform.Triangle) : Waveform.Triangle;

                    (double a, double d, double s, double r) = p.Genre switch
                    {
                        Genre.SynthPop => (0.01, 0.05, 0.45, 0.10),
                        _ => (0.02, 0.10, 0.30, 0.12)
                    };

                    double len = stepDur * (rnd.NextDouble() < 0.25 ? 2.0 : 1.0);
                    if (len > p.Beat * 2) len = p.Beat * 2;

                    mixer.AddMixerInput(MakeNote(wf, f, wfType, start, len, p.Melody * 0.85f, a, d, s, r));
                }

                time += stepDur;
                stepIndex++;
            }
        }

        private static void AddDrums(MixingSampleProvider mixer, Plan p, WaveFormat wf, int durationSeconds, Random rnd)
        {
            if (p.Genre == Genre.Ambient)
            {
                double step = p.Beat / 2.0;
                int i = 0;
                for (double t = 0; t < durationSeconds; t += step, i++)
                {
                    if (rnd.NextDouble() < 0.55) continue;
                    mixer.AddMixerInput(MakeHat(wf, t, 0.12, p.Drums * 0.65f));
                }
                return;
            }

            double stepDur = p.Beat / 2.0;
            for (int i = 0; ; i++)
            {
                double nominal = i * stepDur;
                if (nominal >= durationSeconds) break;

                double t = ApplySwing(p, nominal, stepDur, i);
                int pos = i % 8;

                bool kick = false;
                bool snare = false;
                bool hat = false;

                switch (p.DrumStyle)
                {
                    case 0:
                        kick = (pos == 0) || (pos == 4 && rnd.NextDouble() < 0.7);
                        snare = (pos == 4);
                        hat = (pos % 2 == 1);
                        break;
                    case 1:
                        kick = (pos == 0) || (pos == 3 && rnd.NextDouble() < 0.5) || (pos == 5 && rnd.NextDouble() < 0.45);
                        snare = (pos == 4) || (pos == 6 && rnd.NextDouble() < 0.4);
                        hat = (pos % 2 == 1) || (pos == 0 && rnd.NextDouble() < 0.35);
                        break;
                    case 2:
                        kick = (pos == 0) || (pos == 5 && rnd.NextDouble() < 0.6);
                        snare = (pos == 4);
                        hat = rnd.NextDouble() < 0.65;
                        break;
                    default:
                        kick = (pos == 0) || (pos == 4);
                        snare = (pos == 4) || (pos == 6 && rnd.NextDouble() < 0.5);
                        hat = (pos % 2 == 1) || (rnd.NextDouble() < 0.35);
                        break;
                }

                if (kick) mixer.AddMixerInput(MakeKick(wf, t, 0.20, p.Drums * 0.90f, p.Genre));
                if (snare) mixer.AddMixerInput(MakeSnare(wf, t, 0.22, p.Drums * 0.75f));
                if (hat) mixer.AddMixerInput(MakeHat(wf, t, p.Genre == Genre.SynthPop ? 0.10 : 0.12, p.Drums * 0.55f));
            }
        }

        private static ISampleProvider MakeNote(
            WaveFormat wf,
            double freq,
            Waveform waveform,
            double startSeconds,
            double lengthSeconds,
            float gain,
            double a, double d, double s, double r)
        {
            var gen = new SignalGenerator(wf.SampleRate, wf.Channels)
            {
                Gain = 1.0,
                Frequency = freq,
                Type = waveform switch
                {
                    Waveform.Sine => SignalGeneratorType.Sin,
                    Waveform.Saw => SignalGeneratorType.SawTooth,
                    _ => SignalGeneratorType.Triangle
                }
            };

            ISampleProvider src = gen;
            src = new EnvelopeSampleProvider(src, (float)a, (float)d, (float)s, (float)r, (float)lengthSeconds);

            if (waveform == Waveform.Saw)
                src = new SoftClipSampleProvider(src, 1.10f);

            src = new VolumeSampleProvider(src) { Volume = gain };

            var trimmed = new TrimSampleProvider(src, (float)lengthSeconds);
            return new OffsetSampleProvider(trimmed) { DelayBy = TimeSpan.FromSeconds(startSeconds) };
        }

        private static ISampleProvider MakeKick(WaveFormat wf, double startSeconds, double lengthSeconds, float gain, Genre genre)
        {
            float fStart = genre == Genre.SynthPop ? 95f : 75f;
            float fEnd = 38f;

            var kick = new KickSampleProvider(wf, (float)lengthSeconds, fStart, fEnd, 0.01f, 0.10f, 0.0f, 0.89f);
            ISampleProvider src = new VolumeSampleProvider(kick) { Volume = gain };
            return new OffsetSampleProvider(src) { DelayBy = TimeSpan.FromSeconds(startSeconds) };
        }

        private static ISampleProvider MakeSnare(WaveFormat wf, double startSeconds, double lengthSeconds, float gain)
        {
            var sn = new SnareSampleProvider(wf, (float)lengthSeconds, 0.01f, 0.06f, 0.0f, 0.93f);
            ISampleProvider src = new VolumeSampleProvider(new SoftClipSampleProvider(sn, 1.05f)) { Volume = gain };
            return new OffsetSampleProvider(src) { DelayBy = TimeSpan.FromSeconds(startSeconds) };
        }

        private static ISampleProvider MakeHat(WaveFormat wf, double startSeconds, double lengthSeconds, float gain)
        {
            var hat = new HatSampleProvider(wf, (float)lengthSeconds, 0.01f, 0.02f, 0.0f, 0.97f);
            ISampleProvider src = new VolumeSampleProvider(hat) { Volume = gain };
            return new OffsetSampleProvider(src) { DelayBy = TimeSpan.FromSeconds(startSeconds) };
        }

        private static byte[] RenderToWav16(ISampleProvider mix, WaveFormat wf, int durationSeconds, float master)
        {
            var outFormat16 = new WaveFormat(wf.SampleRate, 16, wf.Channels);

            int totalSamples = wf.SampleRate * durationSeconds * wf.Channels;
            float[] buffer = new float[wf.SampleRate * wf.Channels * 1];
            int remaining = totalSamples;

            using var ms = new MemoryStream();
            using (var writer = new WaveFileWriter(ms, outFormat16))
            {
                while (remaining > 0)
                {
                    int need = Math.Min(buffer.Length, remaining);
                    int read = mix.Read(buffer, 0, need);
                    if (read <= 0) break;

                    for (int i = 0; i < read; i++)
                    {
                        float x = buffer[i] * master;

                        x = (float)Math.Tanh(0.85f * x);
                        x = Math.Clamp(x, -1f, 1f);

                        short s16 = (short)Math.Round(x * short.MaxValue);
                        writer.WriteByte((byte)(s16 & 0xFF));
                        writer.WriteByte((byte)((s16 >> 8) & 0xFF));
                    }

                    remaining -= read;
                }
            }

            return ms.ToArray();
        }

        private static double ApplySwing(Plan p, double nominal, double stepDur, int stepIndex)
        {
            if (p.SwingPercent <= 0) return nominal;
            if (stepIndex % 2 == 0) return nominal;
            double swing = Math.Clamp(p.SwingPercent, 0, 30) / 100.0;
            return nominal + (stepDur * swing);
        }

        private static int DegreeToMidi(Plan p, int degree1Based, int octaveOffsetSemis)
        {
            int deg = Math.Clamp(degree1Based, 1, 7) - 1;
            return p.RootMidi + p.Scale[deg] + octaveOffsetSemis;
        }

        private static double MidiToFreq(int midi)
        {
            return 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
        }

        private sealed class TrimSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider _source;
            private readonly int _maxSamples;
            private int _done;

            public TrimSampleProvider(ISampleProvider source, float seconds)
            {
                _source = source;
                _maxSamples = Math.Max(0, (int)(seconds * source.WaveFormat.SampleRate * source.WaveFormat.Channels));
            }

            public WaveFormat WaveFormat => _source.WaveFormat;

            public int Read(float[] buffer, int offset, int count)
            {
                if (_done >= _maxSamples) return 0;
                int want = Math.Min(count, _maxSamples - _done);
                int read = _source.Read(buffer, offset, want);
                _done += read;
                return read;
            }
        }

        private sealed class EnvelopeSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider _src;
            private readonly float _a, _d, _s, _r, _len;
            private int _pos;

            public EnvelopeSampleProvider(ISampleProvider src, float a, float d, float s, float r, float lenSeconds)
            {
                _src = src;
                _a = Math.Clamp(a, 0f, 1f);
                _d = Math.Clamp(d, 0f, 1f);
                _s = Math.Clamp(s, 0f, 1f);
                _r = Math.Clamp(r, 0f, 1f);
                _len = Math.Max(0.001f, lenSeconds);
            }

            public WaveFormat WaveFormat => _src.WaveFormat;

            public int Read(float[] buffer, int offset, int count)
            {
                int read = _src.Read(buffer, offset, count);
                if (read <= 0) return 0;

                int sr = WaveFormat.SampleRate;
                int ch = WaveFormat.Channels;

                float attackT = _len * _a;
                float decayT = _len * _d;
                float releaseT = _len * _r;
                float sustainStart = attackT + decayT;
                float sustainEnd = Math.Max(sustainStart, _len - releaseT);

                for (int i = 0; i < read; i++)
                {
                    float t = (_pos / (float)(sr * ch));
                    float env;

                    if (t < 0) env = 0;
                    else if (t < attackT && attackT > 0) env = t / attackT;
                    else if (t < sustainStart && decayT > 0)
                    {
                        float k = (t - attackT) / decayT;
                        env = 1f + (_s - 1f) * k;
                    }
                    else if (t < sustainEnd) env = _s;
                    else if (t < _len && releaseT > 0)
                    {
                        float k = Math.Clamp((t - sustainEnd) / releaseT, 0f, 1f);
                        env = _s * (1f - k);
                    }
                    else env = 0f;

                    buffer[offset + i] *= env;
                    _pos++;
                }

                return read;
            }
        }

        private sealed class SoftClipSampleProvider : ISampleProvider
        {
            private readonly ISampleProvider _src;
            private readonly float _drive;

            public SoftClipSampleProvider(ISampleProvider src, float drive)
            {
                _src = src;
                _drive = Math.Max(0.01f, drive);
            }

            public WaveFormat WaveFormat => _src.WaveFormat;

            public int Read(float[] buffer, int offset, int count)
            {
                int read = _src.Read(buffer, offset, count);
                for (int i = 0; i < read; i++)
                {
                    float x = buffer[offset + i] * _drive;
                    buffer[offset + i] = (float)Math.Tanh(x);
                }
                return read;
            }
        }

        private sealed class KickSampleProvider : ISampleProvider
        {
            private readonly WaveFormat _wf;
            private readonly int _total;
            private int _n;

            private readonly float _len;
            private readonly float _f0, _f1;
            private readonly float _a, _d, _s, _r;

            public KickSampleProvider(WaveFormat wf, float len, float f0, float f1, float a, float d, float s, float r)
            {
                _wf = wf;
                _len = len;
                _f0 = f0;
                _f1 = f1;
                _a = a; _d = d; _s = s; _r = r;
                _total = (int)(len * wf.SampleRate * wf.Channels);
            }

            public WaveFormat WaveFormat => _wf;

            public int Read(float[] buffer, int offset, int count)
            {
                int remain = _total - _n;
                if (remain <= 0) return 0;

                int take = Math.Min(remain, count);
                int sr = _wf.SampleRate;

                for (int i = 0; i < take; i++)
                {
                    float t = (_n / (float)_wf.Channels) / sr;
                    float k = Math.Clamp(t / _len, 0f, 1f);
                    float f = _f0 + (_f1 - _f0) * k;

                    float env = Env(t, _len, _a, _d, _s, _r);
                    buffer[offset + i] = (float)Math.Sin(2 * Math.PI * f * t) * env * 0.70f;

                    _n++;
                }

                return take;
            }
        }

        private sealed class SnareSampleProvider : ISampleProvider
        {
            private readonly WaveFormat _wf;
            private readonly int _total;
            private int _n;
            private readonly float _len;
            private readonly float _a, _d, _s, _r;

            public SnareSampleProvider(WaveFormat wf, float len, float a, float d, float s, float r)
            {
                _wf = wf;
                _len = len;
                _a = a; _d = d; _s = s; _r = r;
                _total = (int)(len * wf.SampleRate * wf.Channels);
            }

            public WaveFormat WaveFormat => _wf;

            public int Read(float[] buffer, int offset, int count)
            {
                int remain = _total - _n;
                if (remain <= 0) return 0;

                int take = Math.Min(remain, count);
                int sr = _wf.SampleRate;

                for (int i = 0; i < take; i++)
                {
                    float t = (_n / (float)_wf.Channels) / sr;
                    float env = Env(t, _len, _a, _d, _s, _r);

                    float noise = HashNoise(t) * env * 0.55f;
                    float tone = (float)Math.Sin(2 * Math.PI * 180f * t) * env * 0.20f;

                    buffer[offset + i] = (noise + tone) * 0.45f;
                    _n++;
                }

                return take;
            }
        }

        private sealed class HatSampleProvider : ISampleProvider
        {
            private readonly WaveFormat _wf;
            private readonly int _total;
            private int _n;
            private readonly float _len;
            private readonly float _a, _d, _s, _r;

            public HatSampleProvider(WaveFormat wf, float len, float a, float d, float s, float r)
            {
                _wf = wf;
                _len = len;
                _a = a; _d = d; _s = s; _r = r;
                _total = (int)(len * wf.SampleRate * wf.Channels);
            }

            public WaveFormat WaveFormat => _wf;

            public int Read(float[] buffer, int offset, int count)
            {
                int remain = _total - _n;
                if (remain <= 0) return 0;

                int take = Math.Min(remain, count);
                int sr = _wf.SampleRate;

                for (int i = 0; i < take; i++)
                {
                    float t = (_n / (float)_wf.Channels) / sr;
                    float env = Env(t, _len, _a, _d, _s, _r);
                    buffer[offset + i] = HashNoise(t) * env * 0.28f;
                    _n++;
                }

                return take;
            }
        }

        private static float Env(float t, float len, float a, float d, float s, float r)
        {
            a = Math.Clamp(a, 0f, 1f);
            d = Math.Clamp(d, 0f, 1f);
            r = Math.Clamp(r, 0f, 1f);
            s = Math.Clamp(s, 0f, 1f);

            float attackT = len * a;
            float decayT = len * d;
            float releaseT = len * r;

            float sustainStart = attackT + decayT;
            float sustainEnd = Math.Max(sustainStart, len - releaseT);

            if (t < 0) return 0;
            if (t < attackT && attackT > 0) return t / attackT;

            if (t < sustainStart && decayT > 0)
            {
                float k = (t - attackT) / decayT;
                return 1f + (s - 1f) * k;
            }

            if (t < sustainEnd) return s;

            if (t < len && releaseT > 0)
            {
                float k = Math.Clamp((t - sustainEnd) / releaseT, 0f, 1f);
                return s * (1f - k);
            }

            return 0;
        }

        private static float HashNoise(float t)
        {
            double x = Math.Sin((t * 12345.6789) + 0.1234) * 43758.5453;
            return (float)((x - Math.Floor(x)) * 2.0 - 1.0);
        }
    }
}
