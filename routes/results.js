const express = require('express');
const pool = require('../db/connection');
const { verifyToken } = require('../middleware/auth');

const router = express.Router();

// Submit exam results
router.post('/submit', verifyToken, async (req, res) => {
  try {
    const { examId, answers } = req.body;
    const userId = req.userId;

    if (!examId || !answers) {
      return res.status(400).json({ error: 'Exam ID and answers required' });
    }

    // Get all questions for the exam
    const questionsResult = await pool.query(
      'SELECT id, correct_answer FROM questions WHERE exam_id = $1 ORDER BY id',
      [examId]
    );

    const questions = questionsResult.rows;
    let score = 0;

    // Calculate score
    questions.forEach((question) => {
      if (answers[question.id] === question.correct_answer) {
        score++;
      }
    });

    // Save result to database
    const resultQuery = await pool.query(
      'INSERT INTO results (user_id, exam_id, score, total_questions, answers) VALUES ($1, $2, $3, $4, $5) RETURNING id, score, total_questions',
      [userId, examId, score, questions.length, JSON.stringify(answers)]
    );

    const result = resultQuery.rows[0];
    res.json({
      message: 'Result saved successfully',
      result: {
        id: result.id,
        score: result.score,
        totalQuestions: result.total_questions,
        percentage: Math.round((result.score / result.total_questions) * 100)
      }
    });
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to submit results' });
  }
});

// Get user's score history
router.get('/history', verifyToken, async (req, res) => {
  try {
    const userId = req.userId;

    const result = await pool.query(
      `SELECT r.id, e.title, r.score, r.total_questions, r.created_at,
              ROUND((r.score::float / r.total_questions * 100)::numeric, 2) as percentage
       FROM results r
       JOIN exams e ON r.exam_id = e.id
       WHERE r.user_id = $1
       ORDER BY r.created_at DESC`,
      [userId]
    );

    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch score history' });
  }
});

// Get specific result details
router.get('/:resultId', verifyToken, async (req, res) => {
  try {
    const { resultId } = req.params;
    const userId = req.userId;

    const resultQuery = await pool.query(
      'SELECT id, exam_id, score, total_questions, answers, created_at FROM results WHERE id = $1 AND user_id = $2',
      [resultId, userId]
    );

    if (resultQuery.rows.length === 0) {
      return res.status(404).json({ error: 'Result not found' });
    }

    const resultData = resultQuery.rows[0];
    const examId = resultData.exam_id;

    // Get questions with correct answers
    const questionsResult = await pool.query(
      'SELECT id, text, option_a, option_b, option_c, option_d, correct_answer FROM questions WHERE exam_id = $1 ORDER BY id',
      [examId]
    );

    const answers = JSON.parse(resultData.answers);
    const detailedResults = questionsResult.rows.map((question) => ({
      questionId: question.id,
      text: question.text,
      options: {
        A: question.option_a,
        B: question.option_b,
        C: question.option_c,
        D: question.option_d
      },
      userAnswer: answers[question.id],
      correctAnswer: question.correct_answer,
      isCorrect: answers[question.id] === question.correct_answer
    }));

    res.json({
      id: resultData.id,
      score: resultData.score,
      totalQuestions: resultData.total_questions,
      percentage: Math.round((resultData.score / resultData.total_questions) * 100),
      createdAt: resultData.created_at,
      detailedResults
    });
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch result details' });
  }
});

module.exports = router;
