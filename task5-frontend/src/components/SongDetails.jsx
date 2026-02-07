import { useEffect, useRef, useState } from "react";
import { ThumbsUp, Pause, Play, Volume2 } from "lucide-react";

function formatTime(sec) {
  if (!Number.isFinite(sec) || sec < 0) return "0:00";
  const m = Math.floor(sec / 60);
  const s = Math.floor(sec % 60);
  return `${m}:${String(s).padStart(2, "0")}`;
}

export default function SongDetails({ song, locale }) {
  const coverUrl = `https://localhost:7296/api/songs/${song.id}/cover?locale=${locale}`;
  const audioUrl = `https://localhost:7296/api/songs/${song.id}/preview?locale=${locale}`;

  const audioRef = useRef(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [duration, setDuration] = useState(0);
  const [current, setCurrent] = useState(0);

  useEffect(() => {
    setIsPlaying(false);
    setDuration(0);
    setCurrent(0);

    if (audioRef.current) {
      audioRef.current.pause();
      audioRef.current.currentTime = 0;
    }
  }, [audioUrl]);

  const togglePlay = async () => {
    const a = audioRef.current;
    if (!a) return;

    if (a.paused) {
      try {
        await a.play();
      } catch {}
    } else {
      a.pause();
    }
  };

  const onSeek = (e) => {
    const a = audioRef.current;
    if (!a || !duration) return;

    const value = Number(e.target.value);
    a.currentTime = (value / 1000) * duration;
  };

  const progress = duration ? Math.round((current / duration) * 1000) : 0;

  return (
    <div
      style={{
        background: "#fff",
        border: "1px solid #e6e8ec",
        borderRadius: 10,
        padding: 14,
      }}
    >
      <div style={{ display: "flex", gap: 16, alignItems: "flex-start" }}>
        <div style={{ width: 220, flex: "0 0 220px" }}>
          <img
            src={coverUrl}
            alt={`${song.albumTitle} cover`}
            style={{
              width: 220,
              height: 220,
              objectFit: "cover",
              borderRadius: 8,
              display: "block",
            }}
          />
          <div
            style={{
              marginTop: 10,
              display: "flex",
              justifyContent: "center",
            }}
          >
            <div
              style={{
                display: "inline-flex",
                alignItems: "center",
                gap: 6,
                padding: "4px 10px",
                borderRadius: 999,
                background: "#2563eb",
                color: "#fff",
                fontSize: 12,
                fontWeight: 600,
                boxShadow: "0 1px 2px rgba(0,0,0,0.08)",
                userSelect: "none",
              }}
              title="Likes"
            >
              <ThumbsUp size={14} />
              <span>{song.likes ?? 0}</span>
            </div>
          </div>
        </div>

        <div style={{ flex: 1, minWidth: 0 }}>
          <div
            style={{
              fontSize: 25,
              fontWeight: 800,
              lineHeight: 1.15,
              letterSpacing: "-0.01em",
              color: "#111827",
            }}
          >
            {song.songTitle}
          </div>

          <div style={{ marginTop: 12, color: "#6b7280" }}>
            from <strong style={{ color: "#111827" }}>{song.albumTitle}</strong>{" "}
            by <strong style={{ color: "#111827" }}>{song.artist}</strong>
          </div>

          <audio
            ref={audioRef}
            src={audioUrl}
            preload="metadata"
            onLoadedMetadata={() => setDuration(audioRef.current?.duration ?? 0)}
            onTimeUpdate={() => setCurrent(audioRef.current?.currentTime ?? 0)}
            onPlay={() => setIsPlaying(true)}
            onPause={() => setIsPlaying(false)}
            onEnded={() => setIsPlaying(false)}
            style={{ display: "none" }}
          />

          <div
            style={{
              marginTop: 14,
              display: "flex",
              alignItems: "center",
              gap: 10,
            }}
          >
            <button
              type="button"
              onClick={togglePlay}
              aria-label={isPlaying ? "Pause" : "Play"}
              style={{
                width: 28,
                height: 28,
                borderRadius: "50%",
                border: "none",
                background: "#2563eb",
                color: "#fff",
                display: "grid",
                placeItems: "center",
                cursor: "pointer",
                flex: "0 0 auto",
                padding: 0,
                lineHeight: 1,
              }}
            >
              {isPlaying ? <Pause size={12} /> : <Play size={12} />}
            </button>

            <div
              title="Volume"
              aria-label="Volume"
              style={{
                color: "#9ca3af",
                display: "grid",
                placeItems: "center",
                flex: "0 0 auto",
                userSelect: "none",
              }}
            >
              <Volume2 size={14} />
            </div>

            <input
              type="range"
              min={0}
              max={1000}
              value={progress}
              onChange={onSeek}
              style={{
                flex: 1,
                height: 4,
                accentColor: "#9ca3af",
                cursor: "pointer",
              }}
            />

            <div
              style={{
                fontSize: 12,
                color: "#6b7280",
                background: "#e5e7eb",
                padding: "2px 8px",
                borderRadius: 999,
                whiteSpace: "nowrap",
                flex: "0 0 auto",
              }}
            >
              {formatTime(duration)}
            </div>
          </div>

          <div style={{ marginTop: 12, color: "#6b7280" }}>(Lyrics will be here)</div>
        </div>
      </div>
    </div>
  );
}
