const express = require('express');
const pool = require('../db/connection');
const { verifyToken } = require('../middleware/auth');

const router = express.Router();

// Get all exams (for students)
router.get('/', async (req, res) => {
  try {
    const result = await pool.query('SELECT id, title, description, duration FROM exams ORDER BY created_at DESC');
    res.json(result.rows);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch exams' });
  }
});

// Get exam details with questions
router.get('/:examId', verifyToken, async (req, res) => {
  try {
    const { examId } = req.params;

    const examResult = await pool.query('SELECT id, title, description, duration FROM exams WHERE id = $1', [examId]);
    if (examResult.rows.length === 0) {
      return res.status(404).json({ error: 'Exam not found' });
    }

    const questionsResult = await pool.query(
      'SELECT id, text, option_a, option_b, option_c, option_d FROM questions WHERE exam_id = $1 ORDER BY id',
      [examId]
    );

    const exam = examResult.rows[0];
    exam.questions = questionsResult.rows;

    res.json(exam);
  } catch (error) {
    console.error(error);
    res.status(500).json({ error: 'Failed to fetch exam' });
  }
});

module.exports = router;
