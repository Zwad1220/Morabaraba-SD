const games = [
  {
    title: "Morabaraba",
    description: "The traditional African strategy game of 'Cows'. Outsmart your opponent and capture their herd.",
    image: "Morabaraba_picture.png",
    rules: `How to Play: Rules of the Game

1. THE PLACEMENT PHASE
Each player starts with 12 pieces (traditionally called "cows"). Players take turns placing one cow at a time onto any empty intersection on the board. The goal during this phase is to position your cows to form "mills" while blocking your opponent from doing the same.

2. FORMING A MILL
A Mill is formed when you place three of your cows in a straight line—whether horizontally, vertically, or diagonally. Forming a mill is the primary way to gain an advantage, as it allows you to weaken your opponent’s forces.

3. SHOOTING A COW
When you form a mill, you immediately "shoot" (remove) one of your opponent's cows from the board.

The Golden Rule: You cannot shoot a cow that is currently part of a mill unless all of your opponent’s cows are already in mills.

Breaking the Mill: You can move a cow out of an existing mill and move it back on your next turn to "re-form" the mill and shoot another cow.

4. THE MOVEMENT PHASE
Once all 12 cows have been placed on the board, the game shifts to movement. Players take turns moving one cow to an adjacent empty spot along the lines of the board. You continue to try and form mills to remove your opponent's pieces.

5. THE FLYING PHASE
When a player is reduced to only three cows, they enter the "Flying Phase." This player is no longer restricted to adjacent spots; they can "fly" their cows to any empty intersection on the board. This gives a disadvantaged player a tactical boost to force a draw or a comeback.

6. WINNING THE GAME
A player wins under two conditions:

Reduction: The opponent is reduced to only two cows (making it impossible for them to form a mill).

Gridlock: The opponent is unable to make any legal moves (all their pieces are blocked)
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