import { useEffect, useState } from "react";
import SongsTable from "./components/SongsTable.jsx";
import SongsGallery from "./components/SongsGallery.jsx";
import { getSongs } from "./api/songsApi";
import ControlsBar from "./components/ControlsBar.jsx";

function App() {
  const [songs, setSongs] = useState([]);
  const [error, setError] = useState(null);
  const [expandedId, setExpandedId] = useState(null);

  const [seed, setSeed] = useState(1);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [locale, setLocale] = useState("en-US");
  const [likesAvg, setLikesAvg] = useState(5);
  const [viewMode, setViewMode] = useState("table");

  useEffect(() => {
    setError(null);

    getSongs({ seed, page, pageSize, locale, likesAvg })
      .then((data) => setSongs(data.songs))
      .catch((err) => setError(err.message));
  }, [seed, page, pageSize, locale, likesAvg]);

  return (
    <div
      style={{
        minHeight: "100vh",
        background: "#ffffff",
        color: "#111827",
        padding: 20,
      }}
    >
      <div
        style={{
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          gap: 24,
          padding: "28px 16px 20px",
        }}
      >
        <div
          style={{
            flex: 1,
            height: 1,
            background: "linear-gradient(to right, transparent, #e5e7eb)",
            maxWidth: 240,
          }}
        />

        <h1
          style={{
            margin: 0,
            fontSize: 42,
            fontWeight: 800,
            letterSpacing: "-0.02em",
            color: "#111827",
            whiteSpace: "nowrap",
          }}
        >
          Task5 Songs
        </h1>

        <div
          style={{
            flex: 1,
            height: 1,
            background: "linear-gradient(to left, transparent, #e5e7eb)",
            maxWidth: 240,
          }}
        />
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

      {error && <p style={{ color: "red" }}>{error}</p>}

      {viewMode === "table" && (
        <div
          style={{
            background: "#ffffff",
            borderRadius: 12,
            border: "1px solid #e5e7eb",
            overflow: "hidden",
            marginTop: 16,
          }}
        >
          <SongsTable
            songs={songs}
            expandedId={expandedId}
            setExpandedId={setExpandedId}
            locale={locale}
          />
        </div>
      )}

      {viewMode === "gallery" && (
        <SongsGallery
          locale={locale}
          seed={seed}
          likesAvg={likesAvg}
          pageSize={20}
        />
      )}
    </div>
  );
}

export default App;
