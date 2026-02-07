import { useEffect, useState } from "react";
import SongsTable from "./components/SongsTable.jsx";
import SongsGallery from "./components/SongsGallery.jsx";
import { getSongs } from "./api/songsApi";
import ControlsBar from "./components/ControlsBar.jsx";
import { useMediaQuery } from "./hooks/useMediaQuery"; 

function App() {
  const isMobile = useMediaQuery("(max-width: 640px)");
  const isSmall = useMediaQuery("(max-width: 420px)");

  const [songs, setSongs] = useState([]);
  const [error, setError] = useState(null);
  const [expandedId, setExpandedId] = useState(null);

  const [seed, setSeed] = useState(1);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [locale, setLocale] = useState("en-US");
  const [likesAvg, setLikesAvg] = useState(5);
  const [viewMode, setViewMode] = useState("table");

  useEffect(() => {
    setError(null);

    getSongs({ seed, page, pageSize, locale, likesAvg })
      .then((data) => setSongs(data.songs))
      .catch((err) => {
        setError(err.message);
      });
  }, [seed, page, pageSize, locale, likesAvg]);

  const PAGE = {
    padding: isMobile ? 12 : 20,
    maxWidth: 1200,
    margin: "0 auto",
  };

  const HEADER = {
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    gap: isMobile ? 12 : 24,
    padding: isMobile ? "18px 6px 14px" : "28px 16px 20px",
    position: "relative",
  };

  const LINE = (dir) => ({
    flex: 1,
    height: 1,
    background:
      dir === "left"
        ? "linear-gradient(to right, transparent, #e5e7eb)"
        : "linear-gradient(to left, transparent, #e5e7eb)",
    maxWidth: isMobile ? 110 : 240,
  });

  const TITLE = {
    margin: 0,
    fontSize: isSmall ? 26 : isMobile ? 30 : 42,
    fontWeight: 800,
    letterSpacing: "-0.02em",
    color: "#111827",
    whiteSpace: "nowrap",
    lineHeight: 1.1,
  };

  const CONTENT = {
    marginTop: isMobile ? 10 : 14,
  };

  return (
    <div style={PAGE}>
      <div style={HEADER}>
        <div style={LINE("left")} />

        <h1 style={TITLE}>Task5 Songs</h1>

        <div style={LINE("right")} />
      </div>

      <ControlsBar
        page={page}
        setPage={setPage}
        locale={locale}
        setLocale={setLocale}
        seed={seed}
        setSeed={setSeed}
        likesAvg={likesAvg}
        setLikesAvg={setLikesAvg}
        setExpandedId={setExpandedId}
        viewMode={viewMode}
        setViewMode={setViewMode}
      />

      <div style={CONTENT}>
        {error && (
          <div
            style={{
              background: "#fff",
              border: "1px solid #fecaca",
              color: "#b91c1c",
              padding: "10px 12px",
              borderRadius: 10,
              marginTop: 12,
            }}
          >
            {error}
          </div>
        )}

        {viewMode === "table" && (
          <SongsTable
            songs={songs}
            expandedId={expandedId}
            setExpandedId={setExpandedId}
            locale={locale}
          />
        )}

        {viewMode === "gallery" && (
          <SongsGallery locale={locale} seed={seed} likesAvg={likesAvg} pageSize={20} />
        )}
      </div>
    </div>
  );
}

export default App;
