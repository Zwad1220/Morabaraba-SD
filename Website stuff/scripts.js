const games = [
  {
    title: "Morabaraba",
    description: "The traditional African strategy game of 'Cows'. Outsmart your opponent and capture their herd.",
    image: "https://images.unsplash.com/photo-1611195974226-a6a9be9dd763?q=80&w=1200",
    rules: `
      1. THE PLACEMENT PHASE:
      Each player starts with 12 'cows'. Players take turns placing one cow at a time on any empty intersection on the board.

      2. FORMING A MILL:
      If you place three of your cows in a straight line (horizontally, vertically, or diagonally), you have formed a 'mill'. 

      3. SHOOTING A COW:
      When you form a mill, you can 'shoot' (remove) one of your opponent's cows from the board, provided that cow is not currently part of their own mill.

      4. THE MOVEMENT PHASE:
      Once all cows are placed, players take turns moving a cow to an adjacent empty spot. Forming mills still allows you to capture opponent cows.

      5. THE FLYING PHASE:
      When a player is reduced to only three cows, they can 'fly'—meaning they can move their cow to any empty spot on the board, regardless of distance.

      6. WINNING THE GAME:
      You win if your opponent is reduced to two cows or is unable to make a valid move.
    `,
    page: "game.html"
  }
];

// Selections via querySelector
const gameGrid = document.querySelector(".game-grid");
const modal = document.querySelector(".modal");
const modalTitle = document.querySelector(".modal-title");
const modalRules = document.querySelector(".modal-rules");
const closeModal = document.querySelector(".close-btn");
const startBtn = document.querySelector(".start-btn");

let destination = "";

function renderGames() {
  games.forEach(game => {
    const card = document.createElement("div");
    card.classList.add("game-card");

    card.innerHTML = `
      <img src="${game.image}" alt="${game.title}" class="game-image">
      <div class="game-info">
        <h2 class="game-title">${game.title}</h2>
        <p class="game-description">${game.description}</p>
        <button class="play-btn">Play Morabaraba</button>
      </div>
    `;

    // Handle Play Click
    card.querySelector(".play-btn").addEventListener("click", () => {
      modalTitle.textContent = game.title;
      modalRules.textContent = game.rules;
      destination = game.page;
      modal.classList.remove("hidden");
    });

    gameGrid.appendChild(card);
  });
}

// Modal closing logic
closeModal.addEventListener("click", () => {
  modal.classList.add("hidden");
});

// Outside click to close
modal.addEventListener("click", (e) => {
  if (e.target === modal) modal.classList.add("hidden");
});

// Start Game logic
startBtn.addEventListener("click", () => {
  if (destination) {
    window.location.href = destination;
  }
});

// Initialize
renderGames();