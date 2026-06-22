# EExam API

Exam/Quiz Platform - Node.js + Express + PostgreSQL

## Phase 1: Core MVP

### Features Implemented

#### Auth System
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user and get JWT token
- `POST /api/auth/verify` - Verify JWT token

#### Dashboard/Home
- `GET /api/exams` - List all available exams
- `GET /api/results/history` - Get user's score history

#### Exam Engine
- `GET /api/exams/:examId` - Get exam with questions
- Questions include: text, 4 options (A, B, C, D)
- Timer countdown: 30 minutes per exam (frontend responsibility)
- Auto-submit when timer ends (frontend responsibility)

#### Result Page
- `POST /api/results/submit` - Submit exam answers and get score
- `GET /api/results/:resultId` - Get detailed result with correct vs wrong answers

#### Admin APIs
- `POST /api/admin/exam` - Create exam
- `POST /api/admin/question` - Add question to exam
- `GET /api/admin/results` - View all student results

## Setup

### Prerequisites
- Node.js 14+
- PostgreSQL 12+

### Installation

1. Clone the repository
2. Install dependencies:
   ```bash
   npm install
   ```

3. Create `.env` file (copy from `.env.example`):
   ```
   PORT=5000
   DATABASE_URL=postgresql://user:password@localhost:5432/eexam_db
   JWT_SECRET=your_jwt_secret_key_here
   NODE_ENV=development
   ```

4. Create database and run schema:
   ```bash
   psql -U postgres -d eexam_db -f db/schema.sql
   ```

5. Start the server:
   ```bash
   npm run dev
   ```

Server will run on http://localhost:5000

## API Documentation

### Auth Endpoints

#### Register
```
POST /api/auth/register
Body: { username, password }
Response: { message, token, user }
```

#### Login
```
POST /api/auth/login
Body: { username, password }
Response: { message, token, user }
```

#### Verify Token
```
POST /api/auth/verify
Headers: Authorization: Bearer {token}
Response: { valid, user }
```

### Exam Endpoints

#### Get All Exams
```
GET /api/exams
Response: [{ id, title, description, duration }, ...]
```

#### Get Exam Details
```
GET /api/exams/:examId
Headers: Authorization: Bearer {token}
Response: { id, title, description, duration, questions: [{ id, text, option_a, option_b, option_c, option_d }, ...] }
```

### Result Endpoints

#### Submit Result
```
POST /api/results/submit
Headers: Authorization: Bearer {token}
Body: { examId, answers: { questionId: answer, ... } }
Response: { message, result: { id, score, totalQuestions, percentage } }
```

#### Get Score History
```
GET /api/results/history
Headers: Authorization: Bearer {token}
Response: [{ id, title, score, total_questions, percentage, created_at }, ...]
```

#### Get Result Details
```
GET /api/results/:resultId
Headers: Authorization: Bearer {token}
Response: { id, score, totalQuestions, percentage, createdAt, detailedResults: [{ questionId, text, options, userAnswer, correctAnswer, isCorrect }, ...] }
```

### Admin Endpoints

#### Create Exam
```
POST /api/admin/exam
Headers: Authorization: Bearer {token}
Body: { title, description, duration }
Response: { message, exam }
```

#### Add Question
```
POST /api/admin/question
Headers: Authorization: Bearer {token}
Body: { examId, text, optionA, optionB, optionC, optionD, correctAnswer }
Response: { message, question }
```

#### Get All Results
```
GET /api/admin/results
Headers: Authorization: Bearer {token}
Response: [{ id, username, title, score, total_questions, percentage, created_at }, ...]
```

## Next Steps (Phase 2)

- Frontend React setup
- Auth context and localStorage integration
- Dashboard component with exam list
- Exam engine with timer
- Results display page

## License

MIT
