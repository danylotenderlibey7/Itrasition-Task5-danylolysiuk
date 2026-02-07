import React from "react";
import { ChevronDown, RefreshCw, LayoutList, LayoutGrid, ThumbsUp} from "lucide-react";

export default function ControlsBar({
  page,
  setPage,
  locale,
  setLocale,
  setExpandedId,
  seed,
  setSeed,
  likesAvg,
  setLikesAvg,
  viewMode,
  setViewMode,
}) {
  const resetTableState = () => {
    setPage(1);
    setExpandedId(null);
  };

  const randomizeSeed = () => {
    const randomSeed = Math.floor(Math.random() * Number.MAX_SAFE_INTEGER);
    setSeed(randomSeed);
    resetTableState();
  };

  const [likesDraft, setLikesDraft] = React.useState(likesAvg);
  React.useEffect(() => {
        setLikesDraft(likesAvg);
      }, [likesAvg]);

  const likesText = Math.max(0, Math.min(10, Number(likesDraft) || 0)).toFixed(1);
  const [isLocaleOpen, setIsLocaleOpen] = React.useState(false);
  const localeRef = React.useRef(null);

  const BAR = {
    display: "grid",
    gridTemplateColumns: "1fr auto auto",
    alignItems: "center",
    gap: 18,
    padding: "14px 18px",
    background: "#ffffff",
    borderBottom: "1px solid #e5e7eb",
  };

  const LEFT = {
    display: "flex",
    alignItems: "center",
    gap: 18,
    minWidth: 0,
  };

  const FIELD = {
    background: "#ffffff",
    border: "1px solid #e5e7eb",
    borderRadius: 10,
    boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
    padding: "10px 12px",
    height: 72,
    display: "flex",
    flexDirection: "column",
    justifyContent: "center",
    gap: 4,
  };

  const LABEL = {
    fontSize: 12,
    color: "#6b7280",
    lineHeight: "14px",
    userSelect: "none",
  };

  const VALUE_ROW = {
    display: "flex",
    alignItems: "center",
    gap: 10,
    minWidth: 0,
  };

  const SELECT = {
  width: "100%",
  border: "1px solid #e5e7eb",
  borderRadius: 10,
  background: "#fff",
  padding: "10px 34px 10px 12px",
  fontSize: 14,
  fontWeight: 700,
  color: "#111827",
  outline: "none",
  appearance: "none",
  WebkitAppearance: "none",
  MozAppearance: "none",
  lineHeight: "20px",
};


  const INPUT = {
    border: "none",
    outline: "none",
    background: "transparent",
    fontSize: 14,
    fontWeight: 700,
    color: "#111827",
    width: "100%",
    padding: 0,
    margin: 0,
    minWidth: 0,
  };

  const ICON_BTN = {
    width: 48,
    height: 48,
    borderRadius: 10,
    border: "none",
    background: "transparent",
    display: "grid",
    placeItems: "center",
    cursor: "pointer",
    color: "#111827",
  };

  const VIEW_WRAP = {
    display: "inline-flex",
    border: "1px solid #e5e7eb",
    borderRadius: 10,
    overflow: "hidden",
    boxShadow: "0 1px 2px rgba(0,0,0,0.06)",
    background: "#ffffff",
    height: 56,
    alignItems: "center",
  };

  const VIEW_BTN = (active, left) => ({
    width: 56,
    height: 56,
    border: "none",
    cursor: "pointer",
    display: "grid",
    placeItems: "center",
    background: active ? "#2563eb" : "#ffffff",
    color: active ? "#ffffff" : "#6b7280",
    borderRight: left ? "1px solid #e5e7eb" : "none",
  });

  const likesBox = { ...FIELD, width: 260, justifyContent: "flex-start", paddingBottom: 12};

  const DotRow = ({ value }) => {
    const v = Math.max(0, Math.min(10, value));
    return (
      <div
      style={{
        display: "flex",
        justifyContent: "space-between",
        fontSize: 11,
        color: "#6b7280",
        padding: "0 2px",
        marginTop: 6,
        userSelect: "none",
        lineHeight: "12px",
        height: 12,
      }}
    >
      {Array.from({ length: 11 }).map((_, i) => (
        <div key={i} style={{ width: 18, textAlign: "center" }}>
          {i}
        </div>
      ))}
    </div>

    );
  };
    const MARKS = {
      display: "flex",
      justifyContent: "space-between",
      fontSize: 11,
      color: "#6b7280",
      userSelect: "none",
      marginTop: 6,
      padding: "0 2px",
      lineHeight: "12px",
      height: 12,
    };

    const MARK = {
      width: 18,
      textAlign: "center",
    };
    
    const DROPDOWN_BTN = {
      position: "relative",
      width: "100%",
      height: 40,
      border: "1px solid #e5e7eb",
      borderRadius: 10,
      background: "#fff",
      padding: "0 36px 0 12px",
      fontSize: 14,
      fontWeight: 700,
      color: "#111827",
      cursor: "pointer",
      display: "flex",
      alignItems: "center",
      justifyContent: "space-between",
    };

  return (
    <div style={BAR}>
      <div style={LEFT}>
        <div style={{ ...FIELD, width: 220 }}>
          <div style={LABEL}>Language</div>
          <div style={{ position: "relative" }}>
            <select
              value={locale}
              onChange={(e) => {
                setLocale(e.target.value);
                resetTableState();
              }}
              style={{ ...SELECT}}
            >
              <option value="en-US">English (US)</option>
              <option value="uk-UA">Ukrainian (UA)</option>
            </select>

            <span
            style={{
              position: "absolute",
              right: 10,
              top: "50%",
              transform: "translateY(-50%)",
              color: "#6b7280",
              pointerEvents: "none",
              display: "grid",
              placeItems: "center",
            }}
          >
            <ChevronDown size={16} />
          </span>
          </div>
        </div>

        <div style={{ ...FIELD, width: 200 }}>
          <div style={LABEL}>Seed</div>
          <div style={VALUE_ROW}>
            <input
              type="text"
              inputMode="numeric"
              value={seed}
              onChange={(e) => {
                const raw = e.target.value.trim();
                if (raw === "") return;
                const num = Number(raw);
                if (!Number.isFinite(num) || num < 0) return;
                setSeed(Math.floor(num));
                resetTableState();
              }}
              style={INPUT}
            />

            <button
              type="button"
              onClick={randomizeSeed}
              title="Random seed"
              aria-label="Random seed"
              style={ICON_BTN}
              onMouseEnter={(e) => (e.currentTarget.style.background = "#f3f4f6")}
              onMouseLeave={(e) => (e.currentTarget.style.background = "transparent")}
            >
              <RefreshCw size={18} />
            </button>
          </div>
        </div>

        <div style={likesBox}>
          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between" }}>
          <div style={LABEL}>Likes</div>

          <div
            style={{
              display: "inline-flex",
              alignItems: "center",
              gap: 6,
              padding: "4px 8px",
              border: "1px solid #e5e7eb",
              borderRadius: 999,
              fontSize: 12,
              fontWeight: 800,
              color: "#111827",
              background: "#fff",
            }}
            title="Selected likes average"
          >
            <ThumbsUp size={14} />
            <span>{likesText}</span>
          </div>
        </div>
          <DotRow value={likesAvg} />

          <input
          type="range"
          min="0"
          max="10"
          step="0.1"
          value={likesDraft}
          onChange={(e) => {
            const num = Number(e.target.value);
            setLikesDraft(num);
          }}
          onMouseUp={() => {
            setLikesAvg(likesDraft);
            setExpandedId(null);
          }}
          onTouchEnd={() => {
            setLikesAvg(likesDraft);
            setExpandedId(null);
          }}
          onKeyUp={() => {
            setLikesAvg(likesDraft);
            setExpandedId(null);
          }}
          style={{
            width: "100%",
            cursor: "pointer",
            accentColor: "#a855f7",   // фиолетовый как на скрине
            marginTop: 6,
          }}
        />

        </div>

        {viewMode === "table" && (
          <div style={{ ...FIELD, width: 150 }}>
            <div style={LABEL}>&nbsp;</div>
            <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
              <button
                type="button"
                onClick={() => {
                  setExpandedId(null);
                  setPage((p) => Math.max(1, p - 1));
                }}
                style={{
                  border: "1px solid #e5e7eb",
                  background: "#fff",
                  borderRadius: 10,
                  height: 38,
                  padding: "0 10px",
                  cursor: "pointer",
                }}
              >
                Prev
              </button>

              <div style={{ width: 26, textAlign: "center", fontWeight: 800, color: "#111827" }}>{page}</div>

              <button
                type="button"
                onClick={() => {
                  setExpandedId(null);
                  setPage((p) => p + 1);
                }}
                style={{
                  border: "1px solid #e5e7eb",
                  background: "#fff",
                  borderRadius: 10,
                  height: 38,
                  padding: "0 10px",
                  cursor: "pointer",
                }}
              >
                Next
              </button>
            </div>
          </div>
        )}
      </div>

      <div style={VIEW_WRAP}>
        <button
          type="button"
          onClick={() => {
            setViewMode("table");
            setExpandedId(null);
          }}
          style={VIEW_BTN(viewMode === "table", true)}
          title="Table"
          aria-label="Table"
        >
          <LayoutList size={18} />
        </button>

        <button
          type="button"
          onClick={() => {
            setViewMode("gallery");
            setExpandedId(null);
          }}
          style={VIEW_BTN(viewMode === "gallery", false)}
          title="Gallery"
          aria-label="Gallery"
        >
          <LayoutGrid size={18} />
        </button>
      </div>
    </div>
  );
}
