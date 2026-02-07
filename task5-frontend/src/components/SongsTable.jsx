import React from "react";
import SongDetails from "./SongDetails.jsx";

function useMediaQuery(query) {
  const [matches, setMatches] = React.useState(() =>
    typeof window !== "undefined" ? window.matchMedia(query).matches : false
  );

  React.useEffect(() => {
    const m = window.matchMedia(query);
    const onChange = () => setMatches(m.matches);

    if (m.addEventListener) m.addEventListener("change", onChange);
    else m.addListener(onChange);

    setMatches(m.matches);
    return () => {
      if (m.removeEventListener) m.removeEventListener("change", onChange);
      else m.removeListener(onChange);
    };
  }, [query]);

  return matches;
}

export default function SongsTable({ songs, expandedId, setExpandedId, locale }) {
  const isMobile = useMediaQuery("(max-width: 640px)");

  const ellipsis = {
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
  };

  const headStyle = {
    textAlign: "left",
    padding: "14px 8px",
    fontSize: 20,
    fontWeight: 700,
    letterSpacing: "0.04em",
    color: "#15171d",
    textTransform: "uppercase",
    ...ellipsis,
  };

  if (isMobile) {
    return (
      <div style={{ display: "flex", flexDirection: "column" }}>
        {songs.map((song, i) => {
          const isExpanded = expandedId === song.id;

          return (
            <div key={song.id} style={{ borderBottom: "1px solid #e6e8ec" }}>
              <button
                type="button"
                onClick={() => setExpandedId(isExpanded ? null : song.id)}
                style={{
                  width: "100%",
                  textAlign: "left",
                  background: isExpanded ? "#eaf3ff" : "#fff",
                  border: "none",
                  padding: "14px 12px",
                  display: "flex",
                  alignItems: "center",
                  gap: 10,
                  cursor: "pointer",
                }}
              >
                <span style={{ color: isExpanded ? "#2563eb" : "#6b7280", flex: "0 0 auto" }}>
                  <svg
                    width="14"
                    height="14"
                    viewBox="0 0 16 16"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2.2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    style={{
                      display: "block",
                      transition: "transform 160ms ease",
                      transform: isExpanded ? "rotate(90deg)" : "rotate(0deg)",
                      transformOrigin: "50% 50%",
                    }}
                    aria-hidden="true"
                  >
                    <path d="M5 2l6 6-6 6" />
                  </svg>
                </span>

                <div style={{ minWidth: 0, flex: "1 1 auto" }}>
                  <div
                    style={{
                      display: "flex",
                      alignItems: "baseline",
                      gap: 8,
                      minWidth: 0,
                    }}
                  >
                    <div
                      style={{
                        fontSize: 12,
                        fontWeight: 700,
                        color: "#6b7280",
                        flex: "0 0 auto",
                      }}
                    >
                      #{song.index ?? i + 1}
                    </div>

                    <div
                      style={{
                        fontSize: 15,
                        fontWeight: 800,
                        color: "#111827",
                        minWidth: 0,
                        ...ellipsis,
                      }}
                      title={song.songTitle}
                    >
                      {song.songTitle}
                    </div>
                  </div>

                  <div
                    style={{
                      marginTop: 4,
                      fontSize: 13,
                      fontWeight: 600,
                      color: "#374151",
                      ...ellipsis,
                    }}
                    title={song.artist}
                  >
                    {song.artist}
                  </div>

                  <div
                    style={{
                      marginTop: 6,
                      display: "flex",
                      gap: 8,
                      alignItems: "center",
                      minWidth: 0,
                      fontSize: 12,
                      color: "#6b7280",
                    }}
                  >
                    <span style={{ ...ellipsis }} title={song.genre}>
                      {song.genre}
                    </span>
                    <span style={{ color: "#d1d5db" }}>•</span>
                    <span
                      style={{
                        ...ellipsis,
                        color: song.albumTitle === "Single" ? "#9ca3af" : "#6b7280",
                        fontWeight: song.albumTitle === "Single" ? 600 : 500,
                      }}
                      title={song.albumTitle}
                    >
                      {song.albumTitle}
                    </span>
                  </div>
                </div>
              </button>

              {isExpanded && (
                <div style={{ background: "#d8dade" }}>
                  <div style={{ padding: "12px 12px", borderTop: "2px solid #c6c6c6" }}>
                    <SongDetails song={song} locale={locale} />
                  </div>
                </div>
              )}
            </div>
          );
        })}
      </div>
    );
  }

  return (
    <table
      style={{
        width: "100%",
        borderCollapse: "collapse",
        tableLayout: "fixed",
      }}
    >
      <thead>
        <tr
          style={{
            borderBottom: "2px solid #e6e8ec",
            background: "#fff",
          }}
        >
          <th style={{ width: 46 }}></th>
          <th style={{ width: 100, ...headStyle }}>#</th>
          <th style={{ width: "32%", ...headStyle }}>Song</th>
          <th style={{ width: "32%", ...headStyle }}>Artist</th>
          <th style={{ width: "26%", ...headStyle }}>Album</th>
          <th style={{ width: "16%", ...headStyle }}>Genre</th>
        </tr>
      </thead>

      <tbody>
        {songs.map((song, i) => {
          const isExpanded = expandedId === song.id;

          return (
            <React.Fragment key={song.id}>
              <tr
                onClick={() => setExpandedId(isExpanded ? null : song.id)}
                onMouseEnter={(e) => {
                  if (!isExpanded) e.currentTarget.style.background = "#f7f8fa";
                }}
                onMouseLeave={(e) => {
                  e.currentTarget.style.background = isExpanded ? "#eaf3ff" : "transparent";
                }}
                style={{
                  cursor: "pointer",
                  transition: "background 140ms ease",
                  background: isExpanded ? "#eaf3ff" : "transparent",
                }}
              >
                <td
                  style={{
                    padding: "16px 8px",
                    width: 32,
                    color: isExpanded ? "#2563eb" : "#6b7280",
                  }}
                >
                  <svg
                    width="14"
                    height="14"
                    viewBox="0 0 16 16"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="2.2"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                    style={{
                      display: "block",
                      transition: "transform 160ms ease",
                      transform: isExpanded ? "rotate(90deg)" : "rotate(0deg)",
                      transformOrigin: "50% 50%",
                    }}
                    aria-hidden="true"
                  >
                    <path d="M5 2l6 6-6 6" />
                  </svg>
                </td>

                <td
                  style={{
                    padding: "16px 8px",
                    color: "#6b7280",
                    fontSize: 14,
                    fontWeight: 500,
                    ...ellipsis,
                  }}
                >
                  {song.index ?? i + 1}
                </td>

                <td
                  style={{
                    padding: "16px 8px",
                    fontSize: 15,
                    fontWeight: 700,
                    color: "#111827",
                    ...ellipsis,
                  }}
                  title={song.songTitle}
                >
                  {song.songTitle}
                </td>

                <td
                  style={{
                    padding: "16px 8px",
                    fontSize: 14,
                    fontWeight: 600,
                    color: "#374151",
                    ...ellipsis,
                  }}
                  title={song.artist}
                >
                  {song.artist}
                </td>

                <td style={{ padding: "16px 8px", ...ellipsis }} title={song.albumTitle}>
                  <span
                    style={{
                      fontSize: 14,
                      fontWeight: song.albumTitle === "Single" ? 500 : 600,
                      color: song.albumTitle === "Single" ? "#9ca3af" : "#111827",
                    }}
                  >
                    {song.albumTitle}
                  </span>
                </td>

                <td
                  style={{
                    padding: "16px 8px",
                    fontSize: 14,
                    fontWeight: 600,
                    color: "#374151",
                    ...ellipsis,
                  }}
                  title={song.genre}
                >
                  {song.genre}
                </td>
              </tr>

              {isExpanded && (
                <tr>
                  <td colSpan={6} style={{ padding: 0, background: "#d8dade" }}>
                    <div style={{ padding: "14px 16px", borderTop: "2px solid #c6c6c6" }}>
                      <SongDetails song={song} locale={locale} />
                    </div>
                  </td>
                </tr>
              )}
            </React.Fragment>
          );
        })}
      </tbody>
    </table>
  );
}